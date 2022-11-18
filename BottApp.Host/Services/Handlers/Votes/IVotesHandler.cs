using BottApp.Host.SimpleStateMachine;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BottApp.Host.Services.Handlers.Votes;

public interface IVotesHandler
{ 
    string GetTimeEmooji();


    Task<Message> TryEditMessage(
        ITelegramBotClient? botClient,
        CallbackQuery callbackQuery,
        CancellationToken cancellationToken
    );


    Task BotOnCallbackQueryReceived(
        SimpleFSM FSM,
        ITelegramBotClient? botClient,
        CallbackQuery callbackQuery,
        CancellationToken cancellationToken
    );


    Task BotOnMessageReceivedVotes(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken);


    Task BotOnCallbackQueryReceivedVotes(
        ITelegramBotClient? botClient,
        CallbackQuery callbackQuery,
        CancellationToken cancellationToken
    );


    Task BotOnMessageReceived(
        SimpleFSM FSM,
        ITelegramBotClient botClient,
        Message message,
        CancellationToken cancellationToken
    );


    Task UnknownUpdateHandlerAsync(Update update, CancellationToken cancellationToken);


    Task HandlePollingErrorAsync(
        ITelegramBotClient botClient,
        Exception exception,
        CancellationToken cancellationToken
    );
    
    
}