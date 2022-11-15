using System.Net.Mime;
using BottApp.Database;
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
    private readonly IDatabaseContainer _dbContainer;
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
        IDatabaseContainer dbContainer,
        SimpleFSM fsm,
        MainMenuHandler mainMenuHandler,
        VotesHandler votesHandler,
        AuthHandler authHandler,
        AdminChatHandler adminChatHandler
    )
    {
        _botClient = botClient;
        _logger = logger;
        _dbContainer = dbContainer;
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




        if (update.Message?.Chat.Id == _adminChatID)
        {
            handler = update switch
            {
                {Message: { } message} => _adminChatHandler.BotOnMessageReceived(_, message, cancellationToken, _dbContainer),
                {EditedMessage: { } message} => _adminChatHandler.BotOnMessageReceived(_, message, cancellationToken, _dbContainer),
                _ => _adminChatHandler.UnknownUpdateHandlerAsync(update, cancellationToken)
            };
            await handler;
        }
        else
        {
            switch (_fsm.GetCurrentState())
            {
                case State.Auth:
                    handler = update switch
                    {
                        {Message: { } message} => _authHandler.BotOnMessageReceived
                            (_fsm, _, message, cancellationToken, _dbContainer, _adminChatID),
                        _ => throw new Exception()
                    };
                    await handler;
                    break;

                case State.Menu:
                    handler = update switch
                    {
                        {Message: { } message} => _mainMenuHandler.BotOnMessageReceived
                            (_fsm, _, message, cancellationToken, _dbContainer),
                        {EditedMessage: { } message} => _mainMenuHandler.BotOnMessageReceived
                            (_fsm, _, message, cancellationToken, _dbContainer),
                        {CallbackQuery: { } callbackQuery} => _mainMenuHandler.BotOnCallbackQueryReceived
                            (_fsm, _, callbackQuery, cancellationToken, _dbContainer),
                        _ => _mainMenuHandler.UnknownUpdateHandlerAsync(update, cancellationToken)
                    };
                    await handler;
                    break;

                case State.Votes:
                    handler = update switch
                    {
                        {Message: { } message} => _mainMenuHandler.BotOnMessageReceived
                            (_fsm, _, message, cancellationToken, _dbContainer),
                        {EditedMessage: { } message} => _mainMenuHandler.BotOnMessageReceived
                            (_fsm, _, message, cancellationToken, _dbContainer),
                        {CallbackQuery: { } callbackQuery} => _mainMenuHandler.BotOnCallbackQueryReceived
                            (_fsm, _, callbackQuery, cancellationToken, _dbContainer),
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