using BottApp.Database;
using BottApp.Database.Service;
using BottApp.Database.User;
using BottApp.Database.UserMessage;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace BottApp.Host.Handlers;

public class UpdateHandler : AbstractUpdateHandler
{
    private readonly ITelegramBotClient _;
    private readonly ILogger<UpdateHandler> _logger;
    private readonly IDatabaseContainer _databaseContainer;

    public UpdateHandler(
        ITelegramBotClient botClient,
        ILogger<UpdateHandler> logger,
        IDatabaseContainer databaseContainer,
        IHandlerContainer handlerContainer
    ) : base(handlerContainer)
    {
        _ = botClient;
        _logger = logger;
        _databaseContainer = databaseContainer;
    }
    
    public async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken)
    {
        Task? handler;
        var updateMessage = update.Message ?? update.CallbackQuery?.Message;
        
        if(updateMessage is null)
            return;

        if (updateMessage.Chat.Id == AdminSettings.AdminChatId)
        {
             handler = update switch
            {
                {Message:       { } message}       => _handlerContainer.AdminChatHandler.BotOnMessageReceived(_, message, cancellationToken, null),
                {EditedMessage: { } message}       => _handlerContainer.AdminChatHandler.BotOnMessageReceived(_, message, cancellationToken,null),
                {CallbackQuery: { } callbackQuery} => _handlerContainer.AdminChatHandler.BotOnCallbackQueryReceived(_, callbackQuery, cancellationToken,null),
                _                                  => _handlerContainer.AdminChatHandler.UnknownUpdateHandlerAsync(update, cancellationToken)
            };
            return;
        }


        var user = await _databaseContainer.User.FindOneByUid(updateMessage.Chat.Id);
        if (user is null)
        {
            var telegramProfile = new TelegramProfile(
                updateMessage.Chat.Id,
                updateMessage.Chat.FirstName,
                updateMessage.Chat.LastName,
                null);
            
           user = await _databaseContainer.User.CreateUser(telegramProfile);
        }


        
        // var type = updateMessage.Type.ToString();
        // await _databaseContainer.Message.CreateModel(user.Id, updateMessage.Text, type, DateTime.Now);
  
        handler = user.OnState switch
        {
            OnState.Auth => update switch
            {
                {Message:       { } message}       => _handlerContainer.AuthHandler.BotOnMessageReceived(_, message, cancellationToken, user),
                {CallbackQuery: { } callbackQuery} => _handlerContainer.AuthHandler.BotOnCallbackQueryReceived(_, callbackQuery, cancellationToken, user), 
                _                                  => _handlerContainer.MainMenuHandler.UnknownUpdateHandlerAsync(update, cancellationToken)
            },
            
            OnState.Menu => update switch
            {
                {Message:       { } message}       => _handlerContainer.MainMenuHandler.BotOnMessageReceived(_, message, cancellationToken, user),
                {EditedMessage: { } message}       => _handlerContainer.MainMenuHandler.BotOnMessageReceived(_, message, cancellationToken, user),
                {CallbackQuery: { } callbackQuery} => _handlerContainer.MainMenuHandler.BotOnCallbackQueryReceived(_, callbackQuery, cancellationToken, user),
                _                                  => _handlerContainer.MainMenuHandler.UnknownUpdateHandlerAsync(update, cancellationToken)
            },
            OnState.Votes => update switch
            {
                {Message:       { } message}       => _handlerContainer.VotesHandler.BotOnMessageReceived(_, message, cancellationToken, user),
                {EditedMessage: { } message}       => _handlerContainer.VotesHandler.BotOnMessageReceived(_, message, cancellationToken, user),
                {CallbackQuery: { } callbackQuery} => _handlerContainer.VotesHandler.BotOnCallbackQueryReceived(_, callbackQuery, cancellationToken, user), 
                _                                  => _handlerContainer.VotesHandler.UnknownUpdateHandlerAsync(update, cancellationToken)
            },
            OnState.UploadCandidate => update switch
            {
                {Message:       { } message}       => _handlerContainer.CandidateUploadHandler.BotOnMessageReceived(_, message, cancellationToken, user),
                {EditedMessage: { } message}       => _handlerContainer.CandidateUploadHandler.BotOnMessageReceived(_, message, cancellationToken, user),
                {CallbackQuery: { } callbackQuery} => _handlerContainer.CandidateUploadHandler.BotOnCallbackQueryReceived(_, callbackQuery, cancellationToken, user),
                _                                  => _handlerContainer.CandidateUploadHandler.UnknownUpdateHandlerAsync(update, cancellationToken)
            },
            OnState.Help => update switch
            {
                {Message:       { } message}       => _handlerContainer.HelpHandler.BotOnMessageReceived(_, message, cancellationToken, user),
                {EditedMessage: { } message}       => _handlerContainer.HelpHandler.BotOnMessageReceived(_, message, cancellationToken, user),
                {CallbackQuery: { } callbackQuery} => _handlerContainer.HelpHandler.BotOnCallbackQueryReceived(_, callbackQuery, cancellationToken, user),
                _                                  => _handlerContainer.HelpHandler.UnknownUpdateHandlerAsync(update, cancellationToken)
            },
            
            _ => throw new ArgumentOutOfRangeException()
        };
        
      //  var thread = new Thread(async () => await handler);
      


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