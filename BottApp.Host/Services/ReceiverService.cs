using BottApp.Host.Abstract;
using Telegram.Bot;
using Telegram.Bot.Polling;

namespace BottApp.Host.Services;
public class ReceiverService : ReceiverServiceBase<IUpdateHandler>
{
    public ReceiverService
    (
        ITelegramBotClient botClient,
        IUpdateHandler updateHandler,
        ILogger<ReceiverServiceBase<IUpdateHandler>> logger)
        : base(botClient, updateHandler, logger)
    {
    }
}
