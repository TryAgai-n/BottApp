using BottApp.Database.Document;
using BottApp.Database.User;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BottApp.Host.Services.Handlers.UploadHandler;

public interface ICandidateUploadHandler: IHandler
{
    Task OnStart(ITelegramBotClient botClient, Message message);
}