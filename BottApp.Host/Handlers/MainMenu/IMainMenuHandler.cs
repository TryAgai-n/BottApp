using Telegram.Bot;
using Telegram.Bot.Types;

namespace BottApp.Host.Handlers.MainMenu;

public interface IMainMenuHandler : IHandler
{
    Task OnStart(ITelegramBotClient botClient, Message message);
}