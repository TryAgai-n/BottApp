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
using Telegram.Bot.Types.InputFiles;


namespace BottApp.Host.Services.Handlers.Votes;

public class VotesHandler : IVotesHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IDocumentRepository _documentRepository;
    private ILikedDocumentRepository _likedDocumentRepository;
    private readonly IDocumentService _documentService;
    private readonly IMessageService _messageService;
    private readonly StateService _stateService;


    public VotesHandler(
        IUserRepository userRepository,
        IDocumentRepository documentRepository,
        ILikedDocumentRepository likedDocumentRepository,
        IDocumentService documentService,
        IMessageService messageService,
        StateService stateService)
    {
        _userRepository = userRepository;
        _documentRepository = documentRepository;
        _likedDocumentRepository = likedDocumentRepository;
        _documentService = documentService;
        _messageService = messageService;
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

        switch (callbackQuery.Data)
        {
            case nameof(MainVoteButton.AddCandidate):
                await _stateService.Startup(user, OnState.UploadCandidate, botClient, callbackQuery.Message);
                return;
            
            case nameof(MainVoteButton.Back):
                await _stateService.Startup(user, OnState.Menu, botClient, callbackQuery.Message);
                return;
            
            case nameof(MainVoteButton.ToChooseNomination):
                await ChooseNomination(botClient, callbackQuery, cancellationToken, user);
                return;
            
            case nameof(NominationButton.Biggest):
                await ViewCandidates(botClient, callbackQuery, cancellationToken,  InNomination.First, user, 0, true, false);
                return;
            
            case nameof(NominationButton.Smaller):
                await ViewCandidates(botClient, callbackQuery, cancellationToken,  InNomination.Third, user, 0, true, false);
                return;
            
            case nameof(NominationButton.Fastest):
                await ViewCandidates(botClient, callbackQuery, cancellationToken, InNomination.Second, user, 0, true, false);
                return;
            
            case nameof(VotesButton.Right):
                await ViewCandidates(botClient, callbackQuery, cancellationToken, null, user, 1, false, true);
                return;
            
            case nameof(VotesButton.Left):
                await ViewCandidates(botClient, callbackQuery, cancellationToken, null, user, -1, false, true);
                return;
            
            case nameof(VotesButton.ToVotes):
                await BackToVotes(botClient, callbackQuery, cancellationToken, user);
                return;
            
            case nameof(VotesButton.Like):
                await AddLike(botClient, callbackQuery, cancellationToken, user);
                return;
            
            default:
                await TryEditMessage(botClient, callbackQuery, cancellationToken);
                return;
        }
    }
    
    #region TestSomeMethods

    private async Task AddLike(
        ITelegramBotClient botClient,
        CallbackQuery callbackQuery,
        CancellationToken cancellationToken,
        UserModel user
    )
    {
        
        if (await _likedDocumentRepository.CheckLikeByUser(user.Id, user.ViewDocumentID))
        {

            await _messageService.MarkMessageToDelete(
                await botClient.SendTextMessageAsync(
                    chatId: callbackQuery.Message.Chat.Id, text: "Вы уже голосовали за этого кандидата!"
                )
            );
            return;
        }
        
        var model = await _documentRepository.GetOneByDocumentId(user.ViewDocumentID);

        await _documentRepository.IncrementLikeByDocument(model);
        await _likedDocumentRepository.CreateModel(user.Id, user.ViewDocumentID, true);
        
        
        await _messageService.MarkMessageToDelete(
            await botClient.SendTextMessageAsync(
                chatId: callbackQuery.Message.Chat.Id,
                text: "Ваш голос учтен!"
            )
        );
        
        

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
        if (first)
        {
            await _messageService.DeleteMessages(botClient, user.UId, callbackQuery.Message.MessageId);
            
            
            var documents = await _documentRepository.GetListByNomination(nomination,  true);
            var document = documents.FirstOrDefault();
            
            if (document == null)
            {
                await _messageService.DeleteMessages(botClient, user.UId, callbackQuery.Message.MessageId);

                await _messageService.MarkMessageToDelete(
                    await botClient.SendTextMessageAsync(
                        chatId: callbackQuery.Message.Chat.Id,
                        text: "В текущей номинации нет кандидатов :(\nПредлагаю стать первым и добавить своего!"
                    )
                );
                
                await Task.Delay(3000, cancellationToken);

                await ChooseNomination(botClient, callbackQuery, cancellationToken, user);
                return;
            }
            
            user.ViewDocumentID = document.Id;
            await using FileStream fileStream = new(document.Path, FileMode.Open, FileAccess.Read, FileShare.Read);
            
            var dynamicKeyboardMarkup = await new Keyboard().GetDynamicVotesKeyboard(
                documents.Count, documents.Count == 1 ? 1 : documents.Count, nomination);

            await _messageService.MarkMessageToDelete(
                await botClient.SendPhotoAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    photo: new InputOnlineFile(fileStream, "Document"+document.DocumentExtension),
                    caption: $"{document.Caption}",
                    replyMarkup: dynamicKeyboardMarkup,
                    cancellationToken: cancellationToken
                )
            );

            await _documentRepository.IncrementViewByDocument(document);
        }

        if (next)
        {
            await botClient.SendChatActionAsync(
                callbackQuery.Message.Chat.Id, ChatAction.UploadPhoto, cancellationToken: cancellationToken
            );

            var documentModel = await _documentRepository.GetOneByDocumentId(user.ViewDocumentID);
            
            var docList = await _documentRepository.GetListByNomination(documentModel.DocumentNomination, true);
            
            var docId = docList.IndexOf(docList.FirstOrDefault(x => x!.Id == documentModel.Id));
            
            docId += skip;
            if (docId <= -1)
                docId = docList.Count-1;

            if (docId > docList.Count-1)
                docId = 0;

            var documents = await _documentRepository.ListDocumentsByNomination(documentModel.DocumentNomination, docId, 1);

            var document = documents.First();
            user.ViewDocumentID = document.Id;

            await using FileStream fileStream = new(document.Path, FileMode.Open, FileAccess.Read, FileShare.Read);

            var leftButtonOffset = (docId);
            if (leftButtonOffset <= 0)
                leftButtonOffset = docList.Count;

            var rightButtonOffset = (docId+1);
            if (rightButtonOffset > docList.Count+1)
                rightButtonOffset = 1;

            var dynamicKeyboardMarkup = await new Keyboard().GetDynamicVotesKeyboard(
                leftButtonOffset, rightButtonOffset, documentModel.DocumentNomination);

            await botClient.EditMessageMediaAsync(
                chatId: callbackQuery.Message.Chat.Id, messageId: callbackQuery.Message.MessageId,
                media: new InputMediaPhoto(new InputMedia(fileStream, document.DocumentExtension)),
                replyMarkup: dynamicKeyboardMarkup, cancellationToken: cancellationToken
            );

            fileStream.Close();
            
            await botClient.EditMessageCaptionAsync(
                chatId: callbackQuery.Message.Chat.Id, messageId: callbackQuery.Message.MessageId,
                caption: $"{document.Caption}",
                replyMarkup: dynamicKeyboardMarkup,
                cancellationToken: cancellationToken
            );
            
            await _documentRepository.IncrementViewByDocument(document);

        }
    }
    
    

    async Task ChooseNomination(
        ITelegramBotClient botClient,
        CallbackQuery? callbackQuery,
        CancellationToken cancellationToken,
        UserModel user)
    {
        await _messageService.DeleteMessages(botClient, user.UId, callbackQuery.Message.MessageId);

        await _messageService.MarkMessageToDelete(await botClient.SendTextMessageAsync
        (
            chatId: callbackQuery.Message.Chat.Id,
            text: "Меню: Выбор номинации",
            replyMarkup: Keyboard.NominationKeyboard,
            cancellationToken: cancellationToken
        ));
    }
    
    
    
    
    async Task BackToVotes(
        ITelegramBotClient botClient,
        CallbackQuery? callbackQuery,
        CancellationToken cancellationToken,
        UserModel user)
    {
        await _messageService.DeleteMessages(botClient, user.UId, callbackQuery.Message.MessageId);

        await _messageService.MarkMessageToDelete(await botClient.SendTextMessageAsync
        (
            chatId: callbackQuery.Message.Chat.Id,
            text: "Меню: Голосование",
            replyMarkup: Keyboard.MainVotesKeyboard,
            cancellationToken: cancellationToken
        ));
    }


   
    #endregion

    #region Useful
    //Todo: убрать реализации вспомогательных методов редактирования сообщений в MessageManager
    public string GetTimeEmooji()
    {
        string[] emooji = {"🕐", "🕑", "🕒", "🕓", "🕔", "🕕", "🕖", "🕗", "🕘", "🕙", "🕚", "🕛", "🕐", "🕑",};
        var rand = new Random();
        var preparedString = emooji[rand.Next(0, emooji.Length)];
        return preparedString;
    }
    

    public async Task<Message> TryEditMessage(ITelegramBotClient? botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        var viewText = "Такой команды еще нет ";
        var viewExceptionText = "Все сломаделось : ";

        var editText = viewText + GetTimeEmooji();

        try
        {
            try
            {
                return await botClient.EditMessageTextAsync
                (
                    chatId: callbackQuery.Message.Chat.Id,
                    messageId: callbackQuery.Message.MessageId,
                    text: editText,
                    replyMarkup: Keyboard.MainVotesKeyboard,
                    cancellationToken: cancellationToken
                );
            }
            catch
            {
                editText = viewText + GetTimeEmooji();

                return await botClient.EditMessageTextAsync
                (
                    chatId: callbackQuery.Message.Chat.Id,
                    messageId: callbackQuery.Message.MessageId,
                    text: editText,
                    replyMarkup: Keyboard.MainVotesKeyboard,
                    cancellationToken: cancellationToken
                );
            }
        }
        catch (Exception e)
        {
            return await botClient.SendTextMessageAsync
            (
                chatId: callbackQuery.Message.Chat.Id,
                text: viewExceptionText + "\n" + e,
                replyMarkup: Keyboard.MainKeyboard,
                cancellationToken: cancellationToken
            );
        }
    }
    
    public async Task HandlePollingErrorAsync(
        ITelegramBotClient botClient,
        Exception exception,
        CancellationToken cancellationToken
    )
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
        // _logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
        return Task.CompletedTask;
    }
    #endregion
    
    public async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, UserModel user)
    {
        if (message.Text is not { } messageText)
            return;

        await _messageService.MarkMessageToDelete(message);

        var action = messageText switch
        {
            _ => Usage(botClient, message, cancellationToken)
        };

        async Task Usage(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            await _messageService.DeleteMessages(botClient, user.UId, message.MessageId);
            
            await _messageService.MarkMessageToDelete(
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id, text: "Используй вирутальные кнопки", cancellationToken: cancellationToken
                )
            );

            await Task.Delay(1000);
            
            await _messageService.DeleteMessages(botClient, user.UId, message.MessageId);
            
            await _messageService.MarkMessageToDelete(await botClient.SendTextMessageAsync
            (
                chatId: message.Chat.Id,
                text: "Меню: Голосование",
                replyMarkup: Keyboard.MainVotesKeyboard,
                cancellationToken: cancellationToken
            ));
        }
    }

    
}