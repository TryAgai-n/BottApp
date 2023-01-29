using System.Runtime.CompilerServices;
using BottApp.Database;
using BottApp.Database.Document;
using BottApp.Database.Document.Like;
using BottApp.Database.Service;
using BottApp.Database.Service.Keyboards;
using BottApp.Database.User;
using BottApp.Host.Services;
using BottApp.Host.Services.OnStateStart;
using Microsoft.AspNetCore.Components.Forms;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using InputFile = Microsoft.AspNetCore.Components.Forms.InputFile;
// using Telegram.Bot.Types.InputFiles;
using MenuButton = BottApp.Database.Service.Keyboards.MenuButton;

namespace BottApp.Host.Handlers.Votes;

public class VotesHandler : IVotesHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IDocumentRepository _documentRepository;
    private ILikedDocumentRepository _likedDocumentRepository;
    private readonly IDocumentService _documentService;
    private readonly StateService _stateService;


    public VotesHandler(
        IUserRepository userRepository,
        IDocumentRepository documentRepository,
        ILikedDocumentRepository likedDocumentRepository,
        IDocumentService documentService,
        StateService stateService)
    {
        _userRepository = userRepository;
        _documentRepository = documentRepository;
        _likedDocumentRepository = likedDocumentRepository;
        _documentService = documentService;
        _stateService = stateService;
    }


    public async Task OnStart(ITelegramBotClient botClient, Message message)
    {
        await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id, text: "Меню: Голосование", replyMarkup: Keyboard.MainKeyboard
        );
    }
    
    public async Task BotOnCallbackQueryReceived(
        ITelegramBotClient? botClient,
        CallbackQuery callbackQuery,
        CancellationToken cancellationToken,
        UserModel user
    )
    {
        Enum.TryParse<MenuButton>(callbackQuery.Data, out var result);
        var button = result switch
        {
            MenuButton.Back              => _stateService.StartState(user, OnState.Menu, botClient),
            MenuButton.AddCandidate      => AddCandiate(botClient, callbackQuery, cancellationToken, user),
            MenuButton.ChooseNomination  => ChooseNomination(botClient, callbackQuery, cancellationToken, user),
            MenuButton.Votes             => BackToVotes(botClient, callbackQuery, cancellationToken, user),
            MenuButton.Right             => ViewCandidates(botClient, callbackQuery, cancellationToken, null, user, 1, false, true),
            MenuButton.Left              => ViewCandidates(botClient, callbackQuery, cancellationToken, null, user, -1, false, true),
            MenuButton.Like              => AddLike(botClient, callbackQuery, cancellationToken, user),
            MenuButton.BiggestNomination => ViewCandidates(botClient, callbackQuery, cancellationToken, InNomination.First, user, 0, true, false),
            MenuButton.SmallerNomination => ViewCandidates(botClient, callbackQuery, cancellationToken, InNomination.Second, user, 0, true, false),
            MenuButton.FastestNomination => ViewCandidates(botClient, callbackQuery, cancellationToken, InNomination.Third, user, 0, true, false),
            _                            => _stateService.StartState(user, OnState.Menu, botClient),
        };
        
        await button;
    }


    #region VOTE_REGION

     private async Task AddLike(
        ITelegramBotClient botClient,
        CallbackQuery callbackQuery,
        CancellationToken cancellationToken,
        UserModel user
    )
    {
        Message? msg;

        if (await _likedDocumentRepository.CheckLikeByUser(user.Id, user.ViewDocumentId))
        {
            msg = await botClient.SendTextMessageAsync(
                chatId: callbackQuery.Message.Chat.Id,
                text: "Вы уже голосовали за этого кандидата!"
            );

            await Task.Delay(1500, cancellationToken);

            await botClient.DeleteMessageAsync(chatId: callbackQuery.Message.Chat.Id, msg.MessageId);
            return;
        }
        
        var model = await _documentRepository.GetOneByDocumentId(user.ViewDocumentId);

        await _documentRepository.IncrementLikeByDocument(model);
        await _likedDocumentRepository.CreateModel(user.Id, user.ViewDocumentId, true);
        
        msg = await botClient.SendTextMessageAsync(
            chatId: callbackQuery.Message.Chat.Id,
            text: "Засчитано ❤️"
        );
        
        await Task.Delay(1500, cancellationToken);
        
        await botClient.DeleteMessageAsync(chatId: callbackQuery.Message.Chat.Id, msg.MessageId);
    }


   private async Task ViewCandidates(
        ITelegramBotClient botClient,
        CallbackQuery callbackQuery,
        CancellationToken cancellationToken,
        InNomination? nomination,
        UserModel user,
        int skip,
        bool first,
        bool next
    )
    {
        try  
        {
            if (first)
            {
                var documents = await _documentRepository.GetListByNomination(nomination, true);
                var document = documents.FirstOrDefault();

                if (document is null)
                {
                    await botClient.EditMessageTextAsync(
                        chatId: user.UId, messageId: user.ViewMessageId,
                        text: "В текущей номинации нет кандидатов :(\nПредлагаю стать первым и добавить своего!",
                        cancellationToken: cancellationToken
                    );

                    await Task.Delay(3000, cancellationToken);

                    await _stateService.StartState(user, OnState.UploadCandidate, botClient);
                    return;
                }

                await using FileStream fileStream = new(document.Path, FileMode.Open, FileAccess.Read, FileShare.Read);

                await botClient.DeleteMessageAsync(
                    chatId: user.UId, user.ViewMessageId, cancellationToken: cancellationToken);

                await botClient.SendChatActionAsync(
                    user.UId, ChatAction.UploadPhoto, cancellationToken: cancellationToken);

                await Task.Delay(3000, cancellationToken);
      
                var msg = await botClient.SendPhotoAsync(chatId: user.UId, 
                    photo: new Telegram.Bot.Types.InputFile(fileStream, "Document" + document.DocumentExtension),
                    caption: $"1 из {documents.Count}\n{document.Caption}",
                    replyMarkup: Keyboard.VotesKeyboard, cancellationToken: cancellationToken
                );

                await _documentRepository.IncrementViewByDocument(document);
                await _userRepository.ChangeViewDocumentId(user, document.Id);
                await _userRepository.ChangeViewMessageId(user, msg.MessageId);
            }
            if (next)
            {
                await botClient.SendChatActionAsync(user.UId, ChatAction.UploadPhoto, cancellationToken: cancellationToken);
        
                var documentModel = await _documentRepository.GetOneByDocumentId(user.ViewDocumentId);
                var docList = await _documentRepository.GetListByNomination(documentModel.DocumentNomination, true);
                
                var docIndex =  docList.IndexOf(documentModel);
                docIndex += skip;
            
                if (docIndex < 0)
                    docIndex = docList.Count-1;

                if (docIndex > docList.Count-1)
                    docIndex = 0;
            
                var document = docList[docIndex];
                
                await using FileStream fileStream = new(document.Path, FileMode.Open, FileAccess.Read, FileShare.Read);
                var stream = new Telegram.Bot.Types.InputFile(fileStream, document.DocumentExtension);
                var photo = new InputMediaPhoto(stream);
                photo.Caption = $"{docIndex + 1} из {docList.Count}\n{document.Caption}";
                
                await Task.Delay(300, cancellationToken);
                await botClient.EditMessageMediaAsync(
                    chatId: user.UId, 
                    messageId: user.ViewMessageId,
                    media: photo,
                    replyMarkup: Keyboard.VotesKeyboard, cancellationToken: cancellationToken);
                
                fileStream.Close();
                
                await _userRepository.ChangeViewDocumentId(user, document.Id);
                
                await _documentRepository.IncrementViewByDocument(document);
            
            }
        }
        catch (Exception e)                                                      
        {                                                                        
            Console.BackgroundColor = ConsoleColor.Red;                          
            Console.ForegroundColor = ConsoleColor.Black;                        
            Console.WriteLine("\n\nException\n\n" + e);    
            Console.ResetColor();                                                
            throw;                                                               
        }                                                                        

       
    }

    private async Task AddCandiate(
       ITelegramBotClient botClient,
       CallbackQuery? callbackQuery,
       CancellationToken cancellationToken,
       UserModel user)
   {
       if (VoteTurnSwitch.UploadCandidateIsOn)
       {
           await _stateService.StartState(user, OnState.UploadCandidate, botClient);
           return;
       }
       
       await botClient.EditMessageTextAsync(
           chatId: user.UId,
           messageId: user.ViewMessageId,
           text: "Добавить своего кандидата пока нельзя",
           cancellationToken: cancellationToken);
        
       await Task.Delay(2000);
        
       await botClient.EditMessageTextAsync(
           chatId: user.UId,
           messageId: user.ViewMessageId,
           text: "Меню: Голосование",
           replyMarkup: Keyboard.MainVotesKeyboard,
           cancellationToken: cancellationToken);
     
   }
    private async Task ChooseNomination(
        ITelegramBotClient botClient,
        CallbackQuery? callbackQuery,
        CancellationToken cancellationToken,
        UserModel user)
    {
        if (VoteTurnSwitch.VoteIsOn)
        {
            await botClient.EditMessageTextAsync(
                messageId: callbackQuery.Message.MessageId,
                chatId: user.UId,
                text: "Меню: Выбор номинации для голосования",
                replyMarkup: Keyboard.NominationKeyboard
            );
            return;
        }
        await botClient.EditMessageTextAsync(
            chatId: user.UId,
            messageId: user.ViewMessageId,
            text: "Голосование закрыто, подсчитываем результаты :)",
            cancellationToken: cancellationToken);
        
        await Task.Delay(2000);
        
        await botClient.EditMessageTextAsync(
            chatId: user.UId,
            messageId: user.ViewMessageId,
            text: "Меню: Голосование",
            replyMarkup: Keyboard.MainVotesKeyboard,
            cancellationToken: cancellationToken);
        
    }
    async Task BackToVotes(
        ITelegramBotClient botClient,
        CallbackQuery? callbackQuery,
        CancellationToken cancellationToken,
        UserModel user)
    {
        try
        {
            await botClient.EditMessageTextAsync(
                chatId: user.UId,
                messageId: user.ViewMessageId,
                text: "Меню: Голосование",
                replyMarkup: Keyboard.MainVotesKeyboard,
                cancellationToken: cancellationToken);
        }
        catch
        { 
            await botClient.DeleteMessageAsync(user.UId, user.ViewMessageId);
            
            var msg = await botClient.SendTextMessageAsync(
                chatId: user.UId,
                text: "Меню: Голосование",
                replyMarkup: Keyboard.MainVotesKeyboard,
                cancellationToken: cancellationToken);
            
            await _userRepository.ChangeViewMessageId(user, msg.MessageId);
        }
    }

    #endregion
    
   
    
    public async Task HandlePollingErrorAsync(
        ITelegramBotClient botClient,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException =>
                $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };
        // Cooldown in case of network connection error
        if (exception is RequestException)
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    }
    public Task UnknownUpdateHandlerAsync(Update update, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
  
    
    public async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, UserModel? user)
    {
        if (message.Text is not { } messageText)
            return;

        var msg = await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id, text: "Используй вирутальные кнопки", cancellationToken: cancellationToken);
        
        await Task.Delay(1000, cancellationToken);
        
        await botClient.DeleteMessageAsync(msg.Chat.Id, msg.MessageId, cancellationToken: cancellationToken);
        await botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId, cancellationToken: cancellationToken);;
    }

    
}