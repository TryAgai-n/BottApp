using System.Security.Principal;
using BottApp.Database;
using BottApp.Database.Document;
using BottApp.Database.Service;
using BottApp.Database.Service.Keyboards;
using BottApp.Database.User;
using BottApp.Host.Services.Handlers.MainMenu;
using BottApp.Host.Services.OnStateStart;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using MenuButton = BottApp.Database.Service.Keyboards.MenuButton;


namespace BottApp.Host.Services.Handlers.Votes;

public class VotesHandler : IVotesHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IDocumentRepository _documentRepository;
    
    private readonly IDocumentService _documentService;
    private readonly IMessageService _messageService;
    private readonly StateService _stateService;

    private int Skip { get; set; } = 0;
    private DocumentNomination CurrentNomination { get; set; }
    
    

    public VotesHandler(
        IUserRepository userRepository,
        IDocumentRepository documentRepository,
        IDocumentService documentService, IMessageService messageService,
        StateService stateService)
    {
        _userRepository = userRepository;
        _documentRepository = documentRepository;
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
                _messageService.DeleteMessages(botClient);
                await _stateService.Startup(user, OnState.UploadCandidate, botClient, callbackQuery.Message);
                return;
            
            case nameof(MainVoteButton.Back):
                _messageService.DeleteMessages(botClient);
                await _stateService.Startup(user, OnState.Menu, botClient, callbackQuery.Message);
                return;
            
            case nameof(MainVoteButton.ToChooseNomination):
                await ChooseNomination(botClient, callbackQuery, cancellationToken, user);
                return;
            
            
            
            case nameof(NominationButton.ToFirstNomination):
                await ViewFirstCandidate(botClient, callbackQuery, cancellationToken,  DocumentNomination.Biggest);
                return;
            
            case nameof(NominationButton.ToSecondNomination):
                await ViewFirstCandidate(botClient, callbackQuery, cancellationToken,  DocumentNomination.Smaller);
                return;
            
            case nameof(NominationButton.ToThirdNomination):
                await ViewFirstCandidate(botClient, callbackQuery, cancellationToken, DocumentNomination.Fast);
                return;
            
            case nameof(VotesButton.Right):
                await ViewNextCandidate(botClient, callbackQuery, cancellationToken, 1);
                return;
            
            case nameof(VotesButton.Left):
                await ViewNextCandidate(botClient, callbackQuery, cancellationToken, -1);
                return;
            
            
            case nameof(VotesButton.ToVotes):
                _messageService.DeleteMessages(botClient);
                await BackToVotes(botClient, callbackQuery, cancellationToken, user);
                return;
            
            default:
                await TryEditMessage(botClient, callbackQuery, cancellationToken);
                return;
        }
    }
    
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
             await _messageService.MarkMessageToDelete(
                 await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id, text: "Используй вирутальные кнопки", cancellationToken: cancellationToken
                )
            );

            await Task.Delay(1000);
            
            _messageService.DeleteMessages(botClient);
            
            await _messageService.MarkMessageToDelete(await botClient.SendTextMessageAsync
            (
                chatId: message.Chat.Id,
                text: "Меню: Голосование",
                replyMarkup: Keyboard.MainVotesKeyboard,
                cancellationToken: cancellationToken
            ));
        }
    }
    

    #region TestSomeMethods

    async Task ViewFirstCandidate(
        ITelegramBotClient botClient,
        CallbackQuery callbackQuery,
        CancellationToken cancellationToken,
        DocumentNomination nomination)
    {
        await botClient.DeleteMessageAsync(
            callbackQuery.Message.Chat.Id,
            callbackQuery.Message.MessageId,
            cancellationToken: cancellationToken
        );
    
        //ToDo: Кандидатов может не быть в номинации, тогда буит null
        
        CurrentNomination = nomination;
        
        //var firstDocument = await _documentRepository.GetFirstDocumentByPath(DocumentInPath.Votes);

        // var doc =await _documentRepository.GetFirstDocumentByNomination(nomination);
        // if (doc == null)
        // {
        //      await _messageService.MarkMessageToDelete(await botClient.SendTextMessageAsync
        //     (
        //         chatId: callbackQuery.Message.Chat.Id,
        //         text: "Меню: Голосование",
        //         replyMarkup: Keyboard.MainVotesKeyboard,
        //         cancellationToken: cancellationToken
        //     ));
        // }
        // else
        // {
            var firstDocument = await _documentRepository.GetFirstDocumentByNomination(nomination);
            await using FileStream fileStream = new(firstDocument.Path, FileMode.Open, FileAccess.Read, FileShare.Read);
            await botClient.SendPhotoAsync(
                chatId: callbackQuery.Message.Chat.Id,
                photo: new InputOnlineFile(fileStream, firstDocument.DocumentType),
                caption: firstDocument.Caption,
                replyMarkup: Keyboard.VotesKeyboard,
                cancellationToken: cancellationToken);
     //   }
    }
    
    private async Task<Message> ViewNextCandidate(
        ITelegramBotClient botClient,
        CallbackQuery callbackQuery,
        CancellationToken cancellationToken, 
        int skip)
    {
        await botClient.SendChatActionAsync(
            callbackQuery.Message.Chat.Id,
            ChatAction.UploadPhoto,
            cancellationToken: cancellationToken
        );

        var docCount = await _documentRepository.GetListCountByNomination(CurrentNomination);

        Skip += skip;
        
        if (Skip < 0)
            Skip = docCount.Count-1;
        
        if (Skip > docCount.Count-1)
            Skip = 0;
        var pagination = new Pagination(Skip, 1);
        
        
       // var listDocumentsForVotes = await _documentRepository.ListDocumentsByPath(DocumentInPath.Votes);
        var listDocumentsForVotes = await _documentRepository.ListDocumentsByNomination(pagination, CurrentNomination);
        
        var file = listDocumentsForVotes.First();

        await using FileStream fileStream = new(file.Path, FileMode.Open, FileAccess.Read, FileShare.Read);
        
         await botClient.EditMessageMediaAsync(
            chatId: callbackQuery.Message.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            media: new InputMediaPhoto(new InputMedia(fileStream, file.DocumentType)),
            replyMarkup: Keyboard.VotesKeyboard,
            cancellationToken: cancellationToken);
        
         return await botClient.EditMessageCaptionAsync(
            chatId: callbackQuery.Message.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            caption: file.Caption,
            replyMarkup: Keyboard.VotesKeyboard,
            cancellationToken: cancellationToken);
    }

    async Task ChooseNomination(
        ITelegramBotClient botClient,
        CallbackQuery? callbackQuery,
        CancellationToken cancellationToken,
        UserModel user)
    {
        await botClient.DeleteMessageAsync(
            user.UId,
            callbackQuery.Message.MessageId,
            cancellationToken: cancellationToken
        );

        await botClient.SendTextMessageAsync
        (
            chatId: callbackQuery.Message.Chat.Id,
            text: "Меню: Выбор номинации",
            replyMarkup: Keyboard.NominationKeyboard,
            cancellationToken: cancellationToken
        );
    }
    
    
    
    
    async Task BackToVotes(
        ITelegramBotClient botClient,
        CallbackQuery? callbackQuery,
        CancellationToken cancellationToken,
        UserModel user)
    {
        await botClient.DeleteMessageAsync(
            user.UId,
            callbackQuery.Message.MessageId,
            cancellationToken: cancellationToken
        );

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
}