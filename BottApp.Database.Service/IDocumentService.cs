using System.Threading.Tasks;
using BottApp.Database.Document;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BottApp.Database.Service;

public interface IDocumentService
{
    Task UploadFile(Message message, ITelegramBotClient _botClient);

    Task<bool> UploadVoteFile(Message message, ITelegramBotClient _botClient, InNomination inNomination, string? caption);
}