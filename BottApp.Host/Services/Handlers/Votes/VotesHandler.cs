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

    private int Limit { get; set; } = 0;
    
    

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
            chatId: message.Chat.Id, text: "Меню: Голосование", replyMarkup: Keyboard.MainKeyboardMarkup
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
            
            case nameof(VotesButton.Right):
                await NextCandidate(botClient, callbackQuery, cancellationToken, 1);
                return;
            
            case nameof(VotesButton.Left):
                await NextCandidate(botClient, callbackQuery, cancellationToken, -1);
                return;
            
            case nameof(VotesButton.ToVotes):
                await BackToVotes(botClient, callbackQuery, cancellationToken, user);
                return;
            
            case nameof(MainVoteButton.GiveAVote):
                await GiveAVote(botClient, callbackQuery, cancellationToken);
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
        }
    }
    

    #region TestSomeMethods

    async Task<Message> GiveAVote(
        ITelegramBotClient botClient,
        CallbackQuery callbackQuery,
        CancellationToken cancellationToken)
    {
        await botClient.DeleteMessageAsync(
            callbackQuery.Message.Chat.Id,
            callbackQuery.Message.MessageId,
            cancellationToken: cancellationToken
        );

        var firstDocument = await _documentRepository.GetFirstDocumentByPath(DocumentInPath.Votes);

        await using FileStream fileStream = new(firstDocument.Path, FileMode.Open, FileAccess.Read, FileShare.Read);
        
        return await botClient.SendPhotoAsync(
        chatId: callbackQuery.Message.Chat.Id, 
        photo: new InputOnlineFile(fileStream, firstDocument.DocumentType),
        replyMarkup: Keyboard.VotesKeyboardMarkup,
        cancellationToken: cancellationToken);
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

         await botClient.SendTextMessageAsync
        (
            chatId: callbackQuery.Message.Chat.Id,
            text: "Меню: Голосование",
            replyMarkup: Keyboard.MainVotesKeyboardMarkup,
            cancellationToken: cancellationToken
        );
    }


    private async Task<Message> NextCandidate(
        ITelegramBotClient botClient,
        CallbackQuery callbackQuery,
        CancellationToken cancellationToken, 
        int limit
    )
    {
        await botClient.SendChatActionAsync(
            callbackQuery.Message.Chat.Id,
            ChatAction.UploadPhoto,
            cancellationToken: cancellationToken
        );
        
        var docCount = await _documentRepository.GetCountDocumentByPath(DocumentInPath.Votes);

        Limit += limit;
        
        if (Limit < 0)
            Limit = docCount.Count-1;
        
        if (Limit > docCount.Count-1)
            Limit = 0;
        
        var pagination = new Pagination(Limit, 1);
        var listDocumentsForVotes = await _documentRepository.ListDocumentsByPath(pagination, DocumentInPath.Votes);
        var file = listDocumentsForVotes.First();

        await using FileStream fileStream = new(file.Path, FileMode.Open, FileAccess.Read, FileShare.Read);
        
      return await botClient.EditMessageMediaAsync(
          chatId: callbackQuery.Message.Chat.Id,
          messageId: callbackQuery.Message.MessageId,
          media: new InputMediaPhoto(new InputMedia(fileStream, file.DocumentType)),
          replyMarkup: Keyboard.VotesKeyboardMarkup,
          cancellationToken: cancellationToken);
      
       

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
                    replyMarkup: Keyboard.MainVotesKeyboardMarkup,
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
                    replyMarkup: Keyboard.MainVotesKeyboardMarkup,
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
                replyMarkup: Keyboard.MainKeyboardMarkup,
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