using BottApp.Database;
using BottApp.Database.Document;
using BottApp.Database.User;
using BottApp.Host.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
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
    
    
    private List<int> _deleteMessageList = new List<int>();
    public VotesHandler(IUserRepository userRepository, IDocumentRepository documentRepository, DocumentManager documentManager)
    {
        _userRepository = userRepository;
        _documentRepository = documentRepository;
        _documentManager = documentManager;
    }

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


    public async Task BotOnCallbackQueryReceived(
        ITelegramBotClient? botClient,
        CallbackQuery callbackQuery,
        CancellationToken cancellationToken,
        UserModel user
    )
    {
        // _logger.LogInformation("Received inline keyboard callback from: {CallbackQueryId}", callbackQuery.Id);

        var action = callbackQuery.Data.Split(' ')[0] switch
        {
            nameof(MenuButton.ButtonRight) => await NextCandidate(botClient, callbackQuery, cancellationToken),
            nameof(MenuButton.ButtonLeft) => await NextCandidate(botClient, callbackQuery, cancellationToken),

            "ButtonBack" => await BackMainMenuInterface(botClient, callbackQuery, cancellationToken),
            "ButtonBackToVotes" => await BackToVotes(botClient, callbackQuery, cancellationToken),
            "ButtonAddCandidate" => await AddCandidate(botClient, callbackQuery, cancellationToken, user),
            "ButtontGiveAVote" => await GiveAVote(botClient, callbackQuery, cancellationToken),

            _ => await TryEditMessage(botClient, callbackQuery, cancellationToken)
        };
    }
    
    public async Task<Message> BackMainMenuInterface(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        
        var user = await _userRepository.GetOneByUid(callbackQuery.Message.Chat.Id);
        await _userRepository.ChangeOnState(user, OnState.Menu);
        
        await botClient.DeleteMessageAsync(
            chatId: callbackQuery.Message.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            cancellationToken: cancellationToken);

        return await botClient.SendTextMessageAsync
        (
            chatId: callbackQuery.Message.Chat.Id,
            text: "Главное меню",
            replyMarkup: Keyboard.MainKeyboardMarkup,
            cancellationToken: cancellationToken
        );
    }
    public async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, UserModel user)
    {
        
        if (message.Text is not { } messageText)
            return;

        var action = messageText.Split(' ')[0] switch
        {
            _ => Usage(botClient, message, cancellationToken)
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


    public Task UnknownUpdateHandlerAsync(Update update, CancellationToken cancellationToken)
    {
        // _logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
        return Task.CompletedTask;
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

    #region TestSomeMethods

    async Task<Message> GiveAVote(
        ITelegramBotClient botClient,
        CallbackQuery callbackQuery,
        CancellationToken cancellationToken)
    {
        await botClient.DeleteMessageAsync(
            chatId: callbackQuery.Message.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            cancellationToken: cancellationToken);

        return await NextCandidate(botClient, callbackQuery, cancellationToken);
    }

    async Task<Message> AddCandidate(
        ITelegramBotClient botClient,
        CallbackQuery callbackQuery,
        CancellationToken cancellationToken,
        UserModel user
        
        )
    {
        await _userRepository.ChangeOnState(user, OnState.UploadCandidate);

        return await botClient.SendTextMessageAsync
        (
            chatId: user.UId,
            text: "Отправь мне фотографию своего кандидата",
            cancellationToken: cancellationToken
        );
    }

    async Task<Message> BackToVotes(
        ITelegramBotClient botClient,
        CallbackQuery callbackQuery,
        CancellationToken cancellationToken)
    {
        foreach (var message in _deleteMessageList)
        {
            await botClient.DeleteMessageAsync(
                chatId: callbackQuery.Message.Chat.Id,
                messageId: message,
                cancellationToken: cancellationToken);
        }

        _deleteMessageList.Clear();

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
        CancellationToken cancellationToken
    )
    {
        await botClient.SendChatActionAsync(
            callbackQuery.Message.Chat.Id, ChatAction.UploadPhoto, cancellationToken: cancellationToken
        );

        Random rnd = new Random();
       
        // var fileName = filePath.Split(Path.DirectorySeparatorChar).Last();

        var listDocumentsForVotes = await _documentRepository.ListDocumentsByPath(new Pagination(0, 10), DocumentInPath.Votes);


        var ww = listDocumentsForVotes.First();

        await using FileStream fileStream = new(ww.Path, FileMode.Open, FileAccess.Read, FileShare.Read);
        
        _deleteMessageList.Add(callbackQuery.Message.MessageId+1);
        
        return await botClient.SendPhotoAsync(
            chatId: callbackQuery.Message.Chat.Id, 
            photo: new InputOnlineFile(fileStream, ww.DocumentType),
            caption: "Голосуем за кандидата?", replyMarkup: Keyboard.VotesKeyboardMarkup,
            cancellationToken: cancellationToken
        );
    }
    
    #endregion
}