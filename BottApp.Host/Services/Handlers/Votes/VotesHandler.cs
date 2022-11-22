using BottApp.Database;
using BottApp.Database.Document;
using BottApp.Database.User;
using BottApp.Host.Keyboards;
using BottApp.Host.Services.OnStateStart;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Services;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using MenuButton = BottApp.Host.Keyboards.MenuButton;


namespace BottApp.Host.Services.Handlers.Votes;

public class VotesHandler : IVotesHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly DocumentManager _documentManager;
    private readonly MessageManager _messageManager;
    private readonly StateStart _stateStart;
    public VotesHandler(IUserRepository userRepository, IDocumentRepository documentRepository, DocumentManager documentManager,MessageManager messageManager, StateStart stateStart)
    {
        _userRepository = userRepository;
        _documentRepository = documentRepository;
        _documentManager = documentManager;
        _messageManager = messageManager;
        _stateStart = stateStart;
        
    }
    
    public async Task BotOnCallbackQueryReceived(
        ITelegramBotClient? botClient,
        CallbackQuery callbackQuery,
        CancellationToken cancellationToken,
        UserModel user
    )
    {
        // _logger.LogInformation("Received inline keyboard callback from: {CallbackQueryId}", callbackQuery.Id);
        

        
        switch (callbackQuery.Data)
        {
            case nameof(MainVoteButton.AddCandidate):
                await _stateStart.Startup(user, OnState.UploadCandidate, botClient, callbackQuery.Message);
                break;
            
            case nameof(MainVoteButton.Back):
                await _stateStart.Startup(user, OnState.Menu, botClient, callbackQuery.Message);
                break;
        }
        
        var action = callbackQuery.Data switch
        {
            nameof(VotesButton.Right)        => await NextCandidate(botClient, callbackQuery, cancellationToken),
            nameof(VotesButton.Left)         => await NextCandidate(botClient, callbackQuery, cancellationToken),
            nameof(VotesButton.ToVotes)      => await BackToVotes(botClient, callbackQuery, cancellationToken, user),
            nameof(MainVoteButton.GiveAVote) => await GiveAVote(botClient, callbackQuery, cancellationToken),
            _                                => await TryEditMessage(botClient, callbackQuery, cancellationToken)
        };
    }
    
    public async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, UserModel user)
    {
        
        if (message.Text is not { } messageText)
            return;

        var action = messageText switch
        {
            _ => await Usage(botClient, message, cancellationToken)
        };
        
        static async Task<Message> Usage(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            await botClient.SendTextMessageAsync
            (
                chatId: message.Chat.Id,
                text: "Используй вирутальные кнопки",
                cancellationToken: cancellationToken
            );
            await Task.Delay(100);
            return await botClient.SendTextMessageAsync
            (
                chatId: message.Chat.Id,
                text: "Голосование",
                replyMarkup: Keyboard.MainVotesKeyboardMarkup,
                cancellationToken: cancellationToken
            );
        }
    }
    

    #region TestSomeMethods

    async Task<Message> GiveAVote(
        ITelegramBotClient botClient,
        CallbackQuery callbackQuery,
        CancellationToken cancellationToken)
    {

        var firstDocument = await _documentRepository.GetFirstDocumentByPath(DocumentInPath.Votes);

        await using FileStream fileStream = new(firstDocument.Path, FileMode.Open, FileAccess.Read, FileShare.Read);
        
        return await botClient.SendPhotoAsync(
        chatId: callbackQuery.Message.Chat.Id, 
        photo: new InputOnlineFile(fileStream, firstDocument.DocumentType),
        replyMarkup: Keyboard.VotesKeyboardMarkup,
        cancellationToken: cancellationToken);
    }



    async Task<Message> BackToVotes(
        ITelegramBotClient botClient,
        CallbackQuery callbackQuery,
        CancellationToken cancellationToken,
        UserModel user)
    {
        await botClient.DeleteMessageAsync(
            callbackQuery.Message.Chat.Id,
            callbackQuery.Message.MessageId,
            cancellationToken: cancellationToken
        );
        //await _messageManager.DeleteMessages(botClient, user, callbackQuery.Message);
        return await botClient.SendTextMessageAsync
        (
            chatId: callbackQuery.Message.Chat.Id,
            text: "Меню: Голосование",
            replyMarkup: Keyboard.MainVotesKeyboardMarkup,
            cancellationToken: cancellationToken
        );
    }


    async Task<Message> NextCandidate(
        ITelegramBotClient botClient,
        CallbackQuery callbackQuery,
        CancellationToken cancellationToken)
    {

      
        await botClient.SendChatActionAsync(
            callbackQuery.Message.Chat.Id,
            ChatAction.UploadPhoto,
            cancellationToken: cancellationToken);


        var listDocumentsForVotes =
            await _documentRepository.ListDocumentsByPath(new Pagination(0, 10), DocumentInPath.Votes);


        var ww = listDocumentsForVotes.First();

        await using FileStream fileStream = new(ww.Path, FileMode.Open, FileAccess.Read, FileShare.Read);
        
      return await botClient.EditMessageMediaAsync(
          chatId: callbackQuery.Message.Chat.Id,
          messageId: callbackQuery.Message.MessageId,
          media: new InputMediaPhoto(new InputMedia(fileStream, ww.DocumentType)),
          replyMarkup: Keyboard.VotesKeyboardMarkup,
          cancellationToken: cancellationToken);
      
       

    }
    #endregion

    #region Useful

    public string GetTimeEmooji()
    {
        string[] emooji = {"🕐", "🕑", "🕒", "🕓", "🕔", "🕕", "🕖", "🕗", "🕘", "🕙", "🕚", "🕛", "🕐 ", "🕑 ",};
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