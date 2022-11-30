using System.Threading.Tasks;
using BottApp.Database.Document;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BottApp.Database.Service;

public interface IDocumentService
{
    Task UploadFile(Telegram.Bot.Types.Message message, ITelegramBotClient _botClient);

    Task<bool> UploadVoteFile(Telegram.Bot.Types.Message message, ITelegramBotClient _botClient, DocumentNomination documentNomination, string? caption);
}