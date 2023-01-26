using Telegram.Bot;
using Telegram.Bot.Types;

namespace BottApp.Host.Handlers.UploadHandler;

public interface ICandidateUploadHandler: IHandler
{
    Task OnStart(ITelegramBotClient botClient, Message message);
}