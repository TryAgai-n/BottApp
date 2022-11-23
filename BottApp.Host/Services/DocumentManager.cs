using BottApp.Database;
using BottApp.Database.Document;
using BottApp.Database.User;
using BottApp.Host.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace BottApp.Host.Services;

public class DocumentManager
{
    private readonly IUserRepository _userRepository;
    private readonly IDocumentRepository _documentRepository;


    public DocumentManager(IUserRepository userRepository, IDocumentRepository documentRepository)
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
        await _documentRepository.CreateModel(user.Id, documentType, extension, DateTime.Now, destinationFilePath, DocumentInPath.Base);
        ///

        await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath);
        await _botClient.DownloadFileAsync(filePath, fileStream);
        fileStream.Close();


        await _botClient.SendTextMessageAsync(message.Chat.Id, "Спасибо! Ваш документ загружен в базу данных.");
    }

    


    public async Task UploadVoteFile(Message message, ITelegramBotClient _botClient)
    {
        var documentType = message.Type.ToString();
        var fileInfo = await _botClient.GetFileAsync(message.Document.FileId);
        var filePath = fileInfo.FilePath;
        var extension = Path.GetExtension(filePath);

        var rootPath = Directory.GetCurrentDirectory() + "/DATA/Votes";

        var user = await _userRepository.GetOneByUid((int) message.Chat.Id);

        var newPath = Path.Combine(rootPath, user.TelegramFirstName + "___" + user.UId, documentType, extension);

        if (!Directory.Exists(newPath))
        {
            Directory.CreateDirectory(newPath);
        }


        var destinationFilePath =
            newPath + $"/{user.TelegramFirstName}__{Guid.NewGuid().ToString("N")}__{user.UId}__{extension}";

        ///
        await _documentRepository.CreateModel(user.Id, documentType, extension, DateTime.Now, destinationFilePath, DocumentInPath.Votes);
        ///

        await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath);
        await _botClient.DownloadFileAsync(filePath, fileStream);
        fileStream.Close();


        await _botClient.SendTextMessageAsync(message.Chat.Id, "Спасибо! Ваш документ загружен в базу данных.");
    }


    public static async Task<InputOnlineFile> GetOneCandidatePicture()
    {
        Random rnd = new Random();
        string filePath = @"Files/TestPicture" + rnd.Next(5) + ".jpg";
        await using FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var fileName = filePath.Split(Path.DirectorySeparatorChar).Last();
        return new InputOnlineFile(fileStream, fileName);
    }
}