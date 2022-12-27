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
using static System.Enum;
using MenuButton = BottApp.Database.Service.Keyboards.MenuButton;


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
       
        TryParse<MenuButton>(callbackQuery.Data, out var result);
        var startup = result switch
        {
            MenuButton.AddCandidate      => _stateService.Startup(user, OnState.UploadCandidate, botClient, callbackQuery.Message),
            MenuButton.Back              => _stateService.Startup(user, OnState.Menu, botClient, callbackQuery.Message),
            MenuButton.ChooseNomination  => ChooseNomination(botClient, callbackQuery, cancellationToken, user),
            MenuButton.Votes             => BackToVotes(botClient, callbackQuery, cancellationToken, user),
            MenuButton.Right             => ViewCandidates(botClient, callbackQuery, cancellationToken, null, user, 1, false, true),
            MenuButton.Left              => ViewCandidates(botClient, callbackQuery, cancellationToken, null, user, -1, false, true),
            MenuButton.Like              => AddLike(botClient, callbackQuery, cancellationToken, user),
            MenuButton.BiggestNomination => ViewCandidates(botClient, callbackQuery, cancellationToken, InNomination.First, user, 0, true, false),
            MenuButton.SmallerNomination => ViewCandidates(botClient, callbackQuery, cancellationToken, InNomination.Second, user, 0, true, false),
            MenuButton.FastestNomination => ViewCandidates(botClient, callbackQuery, cancellationToken, InNomination.Third, user, 0, true, false),
            _                            => _stateService.Startup(user, OnState.Menu, botClient, callbackQuery.Message),
        };
        await startup;
        
            
    }
    
    #region TestSomeMethods
    private async Task AddLike(
        ITelegramBotClient botClient,
        CallbackQuery callbackQuery,
        CancellationToken cancellationToken,
        UserModel user
    )
    {
        Message? message;

        if (await _likedDocumentRepository.CheckLikeByUser(user.Id, user.ViewDocumentID))
        {
            message = await botClient.SendTextMessageAsync(
                chatId: callbackQuery.Message.Chat.Id, text: "Вы уже голосовали за этого кандидата!"
            );

            await Task.Delay(1000, cancellationToken);

            await botClient.DeleteMessageAsync(chatId: callbackQuery.Message.Chat.Id,
                message.MessageId);
            return;
        }
        
        var model = await _documentRepository.GetOneByDocumentId(user.ViewDocumentID);

        await _documentRepository.IncrementLikeByDocument(model);
        await _likedDocumentRepository.CreateModel(user.Id, user.ViewDocumentID, true);
        
        
        message = await botClient.SendTextMessageAsync(
            chatId: callbackQuery.Message.Chat.Id,
            text: "Ваш голос учтен!"
        );
        
        await Task.Delay(1000, cancellationToken);
        
        await botClient.DeleteMessageAsync(chatId: callbackQuery.Message.Chat.Id,
            message.MessageId);
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
            
            // var dynamicKeyboardMarkup = await new Keyboard().GetDynamicVotesKeyboard(
            //     documents.Count, documents.Count == 1 ? documents.Count : 2, nomination);

            await _messageService.MarkMessageToDelete(
                await botClient.SendPhotoAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    photo: new InputOnlineFile(fileStream, "Document"+document.DocumentExtension),
                    caption: $"1 из {documents.Count}\n{document.Caption}",
                    replyMarkup: Keyboard.VotesKeyboard,
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
          
            var docIndex =  docList.IndexOf(documentModel);
            
            docIndex += skip;
            
            if (docIndex < 0)
                docIndex = docList.Count-1;

            if (docIndex > docList.Count-1)
                docIndex = 0;
            
            var document = docList[docIndex];
            user.ViewDocumentID = document.Id;

            await using FileStream fileStream = new(document.Path, FileMode.Open, FileAccess.Read, FileShare.Read);
            
            await botClient.EditMessageMediaAsync(
                chatId: callbackQuery.Message.Chat.Id, messageId: callbackQuery.Message.MessageId,
                media: new InputMediaPhoto(new InputMedia(fileStream, document.DocumentExtension)),
                replyMarkup: Keyboard.VotesKeyboard, cancellationToken: cancellationToken
            );

            fileStream.Close();
            
            await botClient.EditMessageCaptionAsync(
                chatId: callbackQuery.Message.Chat.Id, messageId: callbackQuery.Message.MessageId,
                caption: $"{docIndex+1} из {docList.Count}\n{document.Caption}",
                replyMarkup: Keyboard.VotesKeyboard,
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
            text: "Меню: Выбор номинации для голосования",
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