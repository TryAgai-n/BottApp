using System.Net.Mime;
using BottApp.Database;
using BottApp.Database.User;
using BottApp.Host.Keyboards;
using BottApp.Host.SimpleStateMachine;
using BottApp.Host.StateMachine;
using Microsoft.Extensions.Logging.Abstractions;
using Stateless;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Services;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace BottApp.Host.Services.Handlers;

public class UpdateHandler : IUpdateHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<UpdateHandler> _logger;
    private readonly IDatabaseContainer _databaseContainer;
    private readonly SimpleFSM _fsm;
    private readonly MainMenuHandler _mainMenuHandler;
    private readonly VotesHandler _votesHandler;
    private readonly AuthHandler _authHandler;
    private readonly AdminChatHandler _adminChatHandler;
    
    private readonly long _adminChatID = -1001824488986;


    public UpdateHandler
    (
        ITelegramBotClient botClient,
        ILogger<UpdateHandler> logger,
        IDatabaseContainer databaseContainer,
        SimpleFSM fsm,
        MainMenuHandler mainMenuHandler,
        VotesHandler votesHandler,
        AuthHandler authHandler,
        AdminChatHandler adminChatHandler
    )
    {
        _botClient = botClient;
        _logger = logger;
        _databaseContainer = databaseContainer;
        _fsm = fsm;
        _mainMenuHandler = mainMenuHandler;
        _votesHandler = votesHandler;
        _authHandler = authHandler;
        _adminChatHandler = adminChatHandler;
        Console.WriteLine("\n class UpdateHandler is Start\n");
    }


    public async Task HandleUpdateAsync(ITelegramBotClient _, Update update, CancellationToken cancellationToken)
    {
        Task? handler;
        
        var updateMessage  = update.Message;

        if (updateMessage == null)
        {
            updateMessage = update.CallbackQuery.Message;
        }
        
        if (updateMessage.Chat.Id == _adminChatID)
        {
            handler = update switch
            {
                {Message: { } message} => _adminChatHandler.BotOnMessageReceived(_, message, cancellationToken, _databaseContainer),
                {EditedMessage: { } message} => _adminChatHandler.BotOnMessageReceived(_, message, cancellationToken, _databaseContainer),
                {CallbackQuery: { } callbackQuery} => _adminChatHandler.BotOnCallbackQueryReceived
                    (_fsm, _, callbackQuery, cancellationToken),
                _ => _adminChatHandler.UnknownUpdateHandlerAsync(update, cancellationToken)
            };
            await handler;
        }
        else
        {
            var findUserByUid = await _databaseContainer.User.FindOneByUid(updateMessage.Chat.Id);
            if (findUserByUid == null)
            {
                findUserByUid = await _databaseContainer.User.CreateUser(updateMessage.Chat.Id, updateMessage.Chat.FirstName, null);
            }
            
            await MessageManager.SaveMessage(_databaseContainer, updateMessage);

            switch (findUserByUid.OnState)
            {
                case OnState.Auth:
                    handler = update switch
                    {
                        {Message: { } message} => _authHandler.BotOnMessageReceived
                            (_fsm, _, message, cancellationToken, _databaseContainer, _adminChatID),
                        _ => throw new Exception()
                    };
                    await handler;
                    break;

                case OnState.Menu:
                    handler = update switch
                    {
                        {Message: { } message} => _mainMenuHandler.BotOnMessageReceived
                            (_fsm, _, message, cancellationToken, _databaseContainer),
                        {EditedMessage: { } message} => _mainMenuHandler.BotOnMessageReceived
                            (_fsm, _, message, cancellationToken, _databaseContainer),
                        {CallbackQuery: { } callbackQuery} => _mainMenuHandler.BotOnCallbackQueryReceived
                            (_fsm, _, callbackQuery, cancellationToken, _databaseContainer),
                        _ => _mainMenuHandler.UnknownUpdateHandlerAsync(update, cancellationToken)
                    };
                    await handler;
                    break;

                case OnState.Votes:
                    handler = update switch
                    {
                        {Message: { } message} => _mainMenuHandler.BotOnMessageReceived
                            (_fsm, _, message, cancellationToken, _databaseContainer),
                        {EditedMessage: { } message} => _mainMenuHandler.BotOnMessageReceived
                            (_fsm, _, message, cancellationToken, _databaseContainer),
                        {CallbackQuery: { } callbackQuery} => _mainMenuHandler.BotOnCallbackQueryReceived
                            (_fsm, _, callbackQuery, cancellationToken, _databaseContainer),
                        _ => _mainMenuHandler.UnknownUpdateHandlerAsync(update, cancellationToken)
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
        

        // Cooldown in case of network connection error
        if (exception is RequestException)
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    }
}