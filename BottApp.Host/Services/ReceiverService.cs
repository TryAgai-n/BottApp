using BottApp.Host.Abstract;
using BottApp.Host.Services.Handlers;
using Telegram.Bot;

namespace BottApp.Host.Services;

// Compose Receiver and MainMenuHandler implementation
public class ReceiverService : ReceiverServiceBase<MainMenuHandler>
{
    public ReceiverService(
        ITelegramBotClient botClient,
        MainMenuHandler mainMenuHandler,
        ILogger<ReceiverServiceBase<MainMenuHandler>> logger)
        : base(botClient, mainMenuHandler, logger)
    {
    }
}
