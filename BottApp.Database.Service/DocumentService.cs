using BottApp.Database.Document;
using BottApp.Database.Service.Keyboards;
using BottApp.Database.User;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BottApp.Database.Service;

public class DocumentService : IDocumentService
{
    private readonly IUserRepository _userRepository;
    private readonly IDocumentRepository _documentRepository;
    private const string VotePath = "/DATA/Votes";

    public DocumentService(IUserRepository userRepository, IDocumentRepository documentRepository)
    {
        _userRepository = userRepository;
        _documentRepository = documentRepository;
    }

    
    public async Task UploadFile(Message message, ITelegramBotClient _botClient)
    {
        var documentType = message.Type.ToString();
        var fileInfo = await _botClient.GetFileAsync(message.Document.FileId);
        var filePath = fileInfo.FilePath;
        var extension = Path.GetExtension(filePath);


        var rootPath = Directory.GetCurrentDirectory() + "/DATA/";

        var user = await _userRepository.GetOneByUid(message.Chat.Id);

        var newPath = Path.Combine(rootPath, user.TelegramFirstName + "___" + user.UId, documentType, extension);

        if (!Directory.Exists(newPath))
        {
            Directory.CreateDirectory(newPath);
        }


        var destinationFilePath = newPath + $"/{user.TelegramFirstName}__{Guid.NewGuid().ToString("N")}__{user.UId}__{extension}";

        ///
        await _documentRepository.CreateModel(user.Id, documentType, extension, DateTime.Now, destinationFilePath, null, DocumentInPath.Base, null);
        ///

        await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath);
        await _botClient.DownloadFileAsync(filePath, fileStream);
        fileStream.Close();


        await _botClient.SendTextMessageAsync(message.Chat.Id, "Спасибо! Ваш документ загружен в базу данных.");
    }

    public async Task<DocumentModel> CreateEmptyDocumentForVotes(int userId, InNomination nomination)
    {
        var empty =  await _documentRepository.CreateEmpty(userId, nomination, DocumentInPath.Votes, DateTime.Now);
        return empty;
    }

    public async Task<bool> UploadVoteFile(UserModel user, DocumentModel document, ITelegramBotClient _botClient, Message message)
    {  
        if (message.Photo == null) return false;
        
        var documentType = message.Type.ToString();
        var fileInfo = await _botClient.GetFileAsync(message.Photo[^1].FileId);
        var filePath = fileInfo.FilePath;
        var extension = Path.GetExtension(filePath);
        var rootPath = Directory.GetCurrentDirectory() + VotePath ;
        var newPath = Path.Combine(rootPath, user.TelegramFirstName + "___" + user.UId, documentType, extension);

        if (!Directory.Exists(newPath))
        {
            Directory.CreateDirectory(newPath);
        }

        var destinationFilePath = newPath + $"/{user.TelegramFirstName}__{Guid.NewGuid().ToString("N")}__{user.UId}__{extension}";
        
        document.Path = destinationFilePath;
        document.DocumentExtension = extension;
        document.DocumentType = message.Type.ToString();
        
        await _botClient.SendPhotoAsync(
            AdminSettings.AdminChatId,
            message.Photo[^1].FileId,
            $"ID: {document.Id} \n" +
            $"Описание: {document.Caption}\n" +
            $"Номинация: {document.DocumentNomination}\n" +
            $"Отправил пользователь ID {user.Id}, UID {user.UId} @{message.Chat.Username}",
            replyMarkup: Keyboard.ApproveDeclineDocumetKeyboard
        );
        
        await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath);
        await _botClient.DownloadFileAsync(filePath, fileStream);
        fileStream.Close();

        return true;
    }
}