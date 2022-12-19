using BottApp.Database;
using BottApp.Database.User;
using BottApp.Host.SimpleStateMachine;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace BottApp.Host.Services.Handlers;

public class UpdateHandler : AbstractUpdateHandler, IUpdateHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<UpdateHandler> _logger;
    private readonly IDatabaseContainer _databaseContainer;


   // private const long AdminChatId = -1001824488986;
    private const long AdminChatId = -1001897483007;


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


    public async Task HandleUpdateAsync(ITelegramBotClient _, Update update, CancellationToken cancellationToken)
    {
        Task? handler;
        var updateMessage  = update.Message ?? update.CallbackQuery?.Message;
        
        if(updateMessage is null)
            return;

        if (updateMessage.Chat.Id == AdminChatId)
        {
            switch (update)
            {
                case { Message: { } message }:
                    handler =  _handlerContainer.AdminChatHandler.BotOnMessageReceived(_, message, cancellationToken);
                    break;
                case { EditedMessage: { } message }:
                    handler = _handlerContainer.AdminChatHandler.BotOnMessageReceived(_, message, cancellationToken);
                    break;
                case { CallbackQuery: { } callbackQuery }:
                    handler = _handlerContainer.AdminChatHandler.BotOnCallbackQueryReceived(_, callbackQuery,
                        cancellationToken);
                    break;
                default:
                    handler = _handlerContainer.AdminChatHandler.UnknownUpdateHandlerAsync(update, cancellationToken);
                    break;
            }

            await handler;
        }
        else
        {
            var telegramProfile = new TelegramProfile(updateMessage.Chat.Id, updateMessage.Chat.FirstName, updateMessage.Chat.LastName, null);
            var user = await _databaseContainer.User.FindOneByUid(updateMessage.Chat.Id) ??
                       await _databaseContainer.User.CreateUser(telegramProfile);

            //Todo: методы сохранения сообщений необходимо вынести в отдельные состояния(опционально)
            
            var type = updateMessage.Type.ToString();
            await _databaseContainer.Message.CreateModel(user.Id, updateMessage.Text, type, DateTime.Now);
            

            switch (user.OnState)
            {
                case OnState.Auth:
                    handler = update switch
                    {
                         { Message: { }  message } =>  _handlerContainer.AuthHandler.BotOnMessageReceived(_, message, cancellationToken, user, AdminChatId),
                        {CallbackQuery: { } callbackQuery} => _handlerContainer.AuthHandler.BotOnMessageReceived(_, callbackQuery.Message, cancellationToken, user, AdminChatId),
                        _ => throw new Exception()
                    };
                    await handler;
                    break;

                case OnState.Menu:
                    handler = update switch
                    {
                        {Message: { } message} => _handlerContainer.MainMenuHandler.BotOnMessageReceived(_, message, cancellationToken, user),
                        {EditedMessage: { } message} => _handlerContainer.MainMenuHandler.BotOnMessageReceived(_, message, cancellationToken, user),
                        {CallbackQuery: { } callbackQuery} => _handlerContainer.MainMenuHandler.BotOnCallbackQueryReceived(_, callbackQuery, cancellationToken, user),
                        _ => _handlerContainer.MainMenuHandler.UnknownUpdateHandlerAsync(update, cancellationToken)
                    };
                    await handler;
                    break;

                case OnState.Votes:
                    handler = update switch
                    {
                        {Message: { } message} =>  _handlerContainer.VotesHandler.BotOnMessageReceived(_, message, cancellationToken, user),
                        {EditedMessage: { } message} => _handlerContainer.VotesHandler.BotOnMessageReceived(_, message, cancellationToken, user),
                        {CallbackQuery: { } callbackQuery} => _handlerContainer.VotesHandler.BotOnCallbackQueryReceived(_, callbackQuery, cancellationToken, user),
                        _ => _handlerContainer.VotesHandler.UnknownUpdateHandlerAsync(update, cancellationToken)
                    };
                    await handler;
                    break;
                
                case OnState.UploadCandidate:
                    handler = update switch
                    {
                        {Message: { } message} =>  _handlerContainer.CandidateUploadHandler.BotOnMessageReceived(_, message, cancellationToken, user),
                        {EditedMessage: { } message} => _handlerContainer.CandidateUploadHandler.BotOnMessageReceived(_, message, cancellationToken, user),
                        {CallbackQuery: { } callbackQuery} => _handlerContainer.CandidateUploadHandler.BotOnCallbackQueryReceived(_, callbackQuery, cancellationToken, user),
                        _ => _handlerContainer.CandidateUploadHandler.UnknownUpdateHandlerAsync(update, cancellationToken)
                    };
                    await handler;
                    break;
            }
        }
    }


    public async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException =>
                $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };
        
        if (exception is RequestException)
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    }
}