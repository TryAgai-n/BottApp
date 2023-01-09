using BottApp.Database.User;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BottApp.Host.Services.Handlers.MainMenu;

public interface IMainMenuHandler : IHandler
{
    string GetTimeEmooji();

    Task OnStart(ITelegramBotClient botClient, Message message);
    Task<Message> TryEditMessage(
        ITelegramBotClient? botClient,
        CallbackQuery callbackQuery,
        CancellationToken cancellationToken
    );
}