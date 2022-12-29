using BottApp.Database.User;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BottApp.Host.Services.Handlers.Auth;

public interface IAuthHandler
{
    Task BotOnMessageReceived(
        ITelegramBotClient botClient,
        Message message,
        CancellationToken cancellationToken,
        UserModel user
    );


    Task BotWelcomeMessage(ITelegramBotClient botClient, Message? message, CancellationToken cancellationToken);
    
    
}