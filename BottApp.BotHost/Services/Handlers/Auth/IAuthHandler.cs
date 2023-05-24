using Telegram.Bot;
using Telegram.Bot.Types;

namespace BottApp.Host.Services.Handlers.Auth;

public interface IAuthHandler
{
    Task BotOnMessageReceivedVotes(
        ITelegramBotClient botClient,
        Message message,
        CancellationToken cancellationToken
    );


    Task BotOnMessageReceived(
        ITelegramBotClient botClient,
        Message message,
        CancellationToken cancellationToken,
        long AdminChatID
    );


    Task RequestContactAndLocation(ITelegramBotClient botClient, Message? message, CancellationToken cancellationToken);
    
    
}