using BottApp.Host.SimpleStateMachine;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BottApp.Host.Services.Handlers.Auth;

public interface IAuthHandler
{
    Task BotOnMessageReceivedVotes(
        SimpleFSM FSM,
        ITelegramBotClient botClient,
        Message message,
        CancellationToken cancellationToken
    );


    Task BotOnMessageReceived(
        SimpleFSM FSM,
        ITelegramBotClient botClient,
        Message message,
        CancellationToken cancellationToken,
        long AdminChatID
    );


    Task RequestContactAndLocation(ITelegramBotClient botClient, Message? message, CancellationToken cancellationToken);
    
    
}