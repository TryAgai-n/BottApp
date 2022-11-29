using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BottApp.Database.Document;
using BottApp.Database.User;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BottApp.Database.Service;

public class DocumentService : IDocumentService
{
    private readonly IUserRepository _userRepository;
    private readonly IDocumentRepository _documentRepository;


    public DocumentService(IUserRepository userRepository, IDocumentRepository documentRepository)
    {
        _userRepository = userRepository;
        _documentRepository = documentRepository;
    }

    
    public async Task UploadFile(Telegram.Bot.Types.Message message, ITelegramBotClient _botClient)
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

    


    public async Task UploadVoteFile(Telegram.Bot.Types.Message message, ITelegramBotClient _botClient)
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
}