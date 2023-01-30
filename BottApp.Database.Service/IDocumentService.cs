using System.Threading.Tasks;
using BottApp.Database.Document;
using BottApp.Database.User;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BottApp.Database.Service;

public interface IDocumentService
{
    Task UploadFile(Message message, ITelegramBotClient _botClient);
    
    Task<DocumentModel> CreateDocument(int userId, InNomination nomination);

    Task<bool> UploadVoteFile(UserModel user, DocumentModel document, ITelegramBotClient _botClient, Message message);
}