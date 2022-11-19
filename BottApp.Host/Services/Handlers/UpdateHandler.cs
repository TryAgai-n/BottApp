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
    

    
    private readonly long _adminChatID = -1001824488986;


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

        if (updateMessage.Chat.Id == _adminChatID)
        {
            handler = update switch
            {
                {Message: { } message} => _handlerContainer.AdminChatHandler.BotOnMessageReceived(_, message, cancellationToken),
                {EditedMessage: { } message} => _handlerContainer.AdminChatHandler.BotOnMessageReceived(_, message, cancellationToken),
                {CallbackQuery: { } callbackQuery} => _handlerContainer.AdminChatHandler.BotOnCallbackQueryReceived
                    (_, callbackQuery, cancellationToken),
                _ => _handlerContainer.AdminChatHandler.UnknownUpdateHandlerAsync(update, cancellationToken)
            };
            await handler;
        }
        else
        {
            var user = await _databaseContainer.User.FindOneByUid(updateMessage.Chat.Id) ?? await _databaseContainer.User.CreateUser(updateMessage.Chat.Id, updateMessage.Chat.FirstName, null);

            string type = updateMessage.Type.ToString();
            await _databaseContainer.Message.CreateModel(user.Id, updateMessage.Text, type, DateTime.Now);
            

            switch (user.OnState)
            {
                case OnState.Auth:
                    handler = update switch
                    {
                        {
                            Message: { } message
                        } => _handlerContainer.AuthHandler.BotOnMessageReceived(_, message, cancellationToken, _adminChatID),
                        _ => throw new Exception()
                    };
                    await handler;
                    break;

                case OnState.Menu:
                    handler = update switch
                    {
                        {Message: { } message} => _handlerContainer.MainMenuHandler.BotOnMessageReceived(
                            _, message, cancellationToken
                        ),
                        {EditedMessage: { } message} => _handlerContainer.MainMenuHandler.BotOnMessageReceived(
                            _, message, cancellationToken
                        ),
                        {CallbackQuery: { } callbackQuery} => _handlerContainer.MainMenuHandler
                            .BotOnCallbackQueryReceived(_, callbackQuery, cancellationToken),
                        _ => _handlerContainer.MainMenuHandler.UnknownUpdateHandlerAsync(update, cancellationToken)
                    };
                    await handler;
                    break;

                case OnState.Votes:
                    handler = update switch
                    {
                        {Message: { } message} =>  _handlerContainer.VotesHandler.BotOnMessageReceived
                            (_, message, cancellationToken),
                        {EditedMessage: { } message} => _handlerContainer.VotesHandler.BotOnMessageReceived
                            (_, message, cancellationToken),
                        {CallbackQuery: { } callbackQuery} => _handlerContainer.VotesHandler.BotOnCallbackQueryReceived
                            (_, callbackQuery, cancellationToken),
                        _ => _handlerContainer.VotesHandler.UnknownUpdateHandlerAsync(update, cancellationToken)
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