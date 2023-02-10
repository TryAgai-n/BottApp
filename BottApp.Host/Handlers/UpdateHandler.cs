using BottApp.Database;
using BottApp.Database.Service;
using BottApp.Database.User;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BottApp.Host.Handlers
{
    public class UpdateHandler : AbstractUpdateHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<UpdateHandler> _logger;
        private readonly IDatabaseContainer _databaseContainer;


        public UpdateHandler(
            ITelegramBotClient botClient,
            ILogger<UpdateHandler> logger,
            IDatabaseContainer databaseContainer,
            IHandlerContainer handlerContainer
        ) : base(handlerContainer)
        {
            _botClient = botClient;
            _logger = logger;
            _databaseContainer = databaseContainer;
        }
        
        public async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken)
        {
            var updateMessage = update.Message ?? update.CallbackQuery?.Message;

            if (updateMessage is null)
                return;
            
            if (updateMessage.Chat.Id == AdminSettings.AdminChatId)
            {
                await HandleAdminChatUpdateAsync(update, cancellationToken);
                return;
            }
            await HandleUserChatUpdateAsync(update, cancellationToken, updateMessage);
        }
        
        private async Task HandleAdminChatUpdateAsync(Update update, CancellationToken cancellationToken)
        {
            IHandler handler = _handlerContainer.AdminChatHandler;
            var action = update switch
            {
                {Message:       { } message} => handler.BotOnMessageReceived(_botClient, message, cancellationToken, null),
                {EditedMessage: { } message} => handler.BotOnMessageReceived(_botClient, message, cancellationToken, null),
                {CallbackQuery: { } callbackQuery} => handler.BotOnCallbackQueryReceived(_botClient, callbackQuery, cancellationToken, null),
                    _ => handler.UnknownUpdateHandlerAsync(update, cancellationToken)
            };
            await action;
        }


        private async Task HandleUserChatUpdateAsync(Update update, CancellationToken cancellationToken, Message updateMessage)
        {
            var user = await _databaseContainer.User.FindOneByUid(updateMessage.Chat.Id) 
                       ?? 
                       await _databaseContainer.User.CreateUser(
                           new TelegramProfile(
                               updateMessage.Chat.Id, 
                               updateMessage.Chat.FirstName,
                               updateMessage.Chat.LastName, null));

            await _databaseContainer.Message.CreateModel(user.Id, updateMessage.Text, updateMessage.Type.ToString(), DateTime.Now);

            if (updateMessage.Text == "/start" && user.isAuthorized)
            {
                await _handlerContainer.MainMenuHandler.BotOnMessageReceived(_botClient, updateMessage, cancellationToken, user);
                return;
            }
            
            IHandler handler = user.OnState switch
            {
                OnState.Auth => _handlerContainer.AuthHandler,
                OnState.Menu => _handlerContainer.MainMenuHandler,
                OnState.Help => _handlerContainer.HelpHandler,
                OnState.Votes => _handlerContainer.VotesHandler,
                OnState.UploadCandidate => _handlerContainer.CandidateUploadHandler,
                _ => throw new ArgumentOutOfRangeException()
            };

            var action = update switch
            {
                {Message:       { } message}       => handler.BotOnMessageReceived(_botClient, message, cancellationToken, user),
                {EditedMessage: { } message}       => handler.BotOnMessageReceived(_botClient, message, cancellationToken, user),
                {CallbackQuery: { } callbackQuery} => handler.BotOnCallbackQueryReceived(_botClient, callbackQuery, cancellationToken, user),
                _                                  => handler.UnknownUpdateHandlerAsync(update, cancellationToken)
            };
            await action;
        }
    }
}

