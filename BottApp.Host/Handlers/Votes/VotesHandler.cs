using BottApp.Database.Document;
using BottApp.Database.Document.Like;
using BottApp.Database.Service;
using BottApp.Database.Service.Keyboards;
using BottApp.Database.User;
using BottApp.Host.Services.OnStateStart;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using InputFile = Telegram.Bot.Types.InputFile;
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
            MenuButton.AddCandidate      => AddCandidate(botClient, callbackQuery, cancellationToken, user),
            MenuButton.ChooseNomination  => ChooseNomination(botClient, callbackQuery, cancellationToken, user),
            MenuButton.Votes             => BackToVotes(botClient, callbackQuery, cancellationToken, user),
            MenuButton.Like              => AddLike(botClient, callbackQuery, cancellationToken, user),
            MenuButton.BiggestNomination => ViewCandidates(botClient, user, 0, cancellationToken, InNomination.First),
            MenuButton.SmallerNomination => ViewCandidates(botClient, user, 0, cancellationToken, InNomination.Second),
            MenuButton.FastestNomination => ViewCandidates(botClient, user, 0, cancellationToken, InNomination.Third),
            MenuButton.Right             => ViewCandidates(botClient, user, 1, cancellationToken, null),
            MenuButton.Left              => ViewCandidates(botClient, user, -1, cancellationToken, null),
            _                            => _stateService.StartState(user, OnState.Menu, botClient),
        };
        
        await button;
    }
  private async Task ViewCandidates(ITelegramBotClient botClient, UserModel user, int skip, CancellationToken cancellationToken, InNomination? nomination)
     {
         if (nomination is not null)
         {
             var document = await _documentRepository.FindFirstDocumentByNomination(nomination);
             if (document is null)
             {
                 await botClient.EditMessageTextAsync(
                     chatId: user.UId, messageId: user.ViewMessageId,
                     text: "There are no candidates in the current nomination :(\nBecome the first to add your own!",
                     cancellationToken: cancellationToken
                 );

                 await Task.Delay(3000, cancellationToken);

                 await _stateService.StartState(user, OnState.UploadCandidate, botClient);
                 return;
             }
             
             await botClient.DeleteMessageAsync(user.UId, user.ViewMessageId, cancellationToken: cancellationToken);
             
             var count = await _documentRepository.GetCountByNomination(nomination);
             var filePath = Directory.GetCurrentDirectory() + document.Path;
             await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
             var stream = new InputFile(fileStream, document.DocumentExtension);
             var caption =  $"1 of {count}\n{document.Caption}";

             var msg = await botClient.SendPhotoAsync(user.UId, stream, caption: caption, replyMarkup: Keyboard.VotesKeyboard, cancellationToken: cancellationToken);
             await _userRepository.ChangeViewDocumentId(user, document.Id);
             await _userRepository.ChangeViewMessageId(user, msg.MessageId);
             return;
         } 
         else
         {
             var currentDocument = await _documentRepository.GetOneByDocumentId(user.ViewDocumentId);
             var documentList = await _documentRepository.GetListByNomination(currentDocument.DocumentNomination, true);
             var currentIndex = documentList.IndexOf(currentDocument);
             currentIndex += skip;

             if (currentIndex < 0)
                 currentIndex = documentList.Count - 1;
             else if (currentIndex >= documentList.Count)
                 currentIndex = 0;
             

             var document = documentList[currentIndex];
             var filePath = Directory.GetCurrentDirectory() + document.Path;

             await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
             var stream = new InputFile(fileStream, document.DocumentExtension);
             var photo = new InputMediaPhoto(stream) {Caption = $"{currentIndex + 1} of {documentList.Count}\n{document.Caption}"};

             await botClient.EditMessageMediaAsync(
                 chatId: user.UId, messageId: user.ViewMessageId, media: photo, replyMarkup: Keyboard.VotesKeyboard,
                 cancellationToken: cancellationToken
             );

             await _documentRepository.IncrementViewByDocument(document);
             await _userRepository.ChangeViewDocumentId(user, document.Id);
         }
           
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


   

    private async Task AddCandidate(
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