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

    public async Task<DocumentModel> CreateDocument(int userId, InNomination nomination)
    {
       return await _documentRepository.CreateModel(userId, nomination, DocumentInPath.Votes, DateTime.Now);
    }

    public async Task<bool> UploadVoteFile(UserModel user, DocumentModel document, ITelegramBotClient _botClient, Message message)
    {  
        var fileInfo = await _botClient.GetFileAsync(message.Photo[^1].FileId);
        var filePath = fileInfo.FilePath;
        if (message.Photo == null) return false;
        
        var documentType = message.Type.ToString();
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
        document.DocumentType = documentType;
        
        await _documentRepository.UpdateDocument(document);
        
        await using FileStream createNewDocument = System.IO.File.OpenWrite(destinationFilePath);
        await _botClient.DownloadFileAsync(filePath, createNewDocument);
        createNewDocument.Close();
        
        
        
        await using FileStream fileStream = new(destinationFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        
        
        var photo = new InputFile(fileStream, "Document");
        
        var caption = $"ID: {document.Id} \n" +
                      $"Описание: {document.Caption}\n" +
                      $"Номинация: {document.DocumentNomination}\n" +
                      $"Отправил пользователь ID {user.Id}, UID {user.UId} @{message.Chat.Username}";
        
        await _botClient.SendPhotoAsync(
            chatId: AdminSettings.AdminChatId,
            photo: photo,
            caption: caption,
            replyMarkup: Keyboard.ApproveDeclineDocumetKeyboard
        );
     
        fileStream.Close();
        return true;
    }
}