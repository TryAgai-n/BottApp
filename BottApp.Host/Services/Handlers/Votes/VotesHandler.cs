using BottApp.Database.Document;
using BottApp.Database.Service;
using BottApp.Database.Service.Keyboards;
using BottApp.Database.User;
using BottApp.Host.Services.OnStateStart;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;


namespace BottApp.Host.Services.Handlers.Votes;

public class VotesHandler : IVotesHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IDocumentRepository _documentRepository;
    
    private readonly IDocumentService _documentService;
    private readonly IMessageService _messageService;
    private readonly StateService _stateService;


    public VotesHandler(
        IUserRepository userRepository,
        IDocumentRepository documentRepository,
        IDocumentService documentService,
        IMessageService messageService,
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
                await _stateService.Startup(user, OnState.UploadCandidate, botClient, callbackQuery.Message);
                return;
            
            case nameof(MainVoteButton.Back):
                await _stateService.Startup(user, OnState.Menu, botClient, callbackQuery.Message);
                return;
            
            case nameof(MainVoteButton.ToChooseNomination):
                await ChooseNomination(botClient, callbackQuery, cancellationToken, user);
                return;
            
            case nameof(NominationButton.Biggest):
                await ViewCandidates(botClient, callbackQuery, cancellationToken,  InNomination.ㅤ, user, 0, true, false);
                return;
            
            case nameof(NominationButton.Smaller):
                await ViewCandidates(botClient, callbackQuery, cancellationToken,  InNomination.ㅤㅤㅤ, user, 0, true, false);
                return;
            
            case nameof(NominationButton.Fastest):
                await ViewCandidates(botClient, callbackQuery, cancellationToken, InNomination.ㅤㅤ, user, 0, true, false);
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

    async Task AddLike(
        ITelegramBotClient botClient,
        CallbackQuery callbackQuery,
        CancellationToken cancellationToken,
        UserModel user)
    {
        
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
            await _messageService.DeleteMessages(botClient, user);
            var docCount = await _documentRepository.GetCountByNomination(nomination);
            var documents = await _documentRepository.ListDocumentsByNomination(nomination, skip);
            var document = documents.FirstOrDefault();

            if (document == null)
            {
                await _messageService.DeleteMessages(botClient, user);

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

            await using FileStream fileStream = new(document.Path, FileMode.Open, FileAccess.Read, FileShare.Read);
            
            var dynamicKeyboardMarkup = await new Keyboard().GetDynamicVotesKeyboard(
                docCount, 2, nomination);

            await _messageService.MarkMessageToDelete(
                await botClient.SendPhotoAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    photo: new InputOnlineFile(fileStream, document.DocumentType),
                    caption: 1 + " " + document.DocumentNomination, replyMarkup: dynamicKeyboardMarkup,
                    cancellationToken: cancellationToken
                )
            );
        }

        if (next)
        {
            await botClient.SendChatActionAsync(
                callbackQuery.Message.Chat.Id, ChatAction.UploadPhoto, cancellationToken: cancellationToken
            );

           // var buttonItem = callbackQuery.Message.ReplyMarkup.InlineKeyboard.Select(x => x.Select(x => x.Text));
            
            
            var captionItem = callbackQuery.Message.Caption.Split(' ');
            var offset = Convert.ToInt32(captionItem[0]);
            var _nomination = (InNomination) Enum.Parse(typeof(InNomination), captionItem[1]);
            var docCount = await _documentRepository.GetCountByNomination(_nomination);

            offset += skip;
            if (offset <= 0)
                offset = docCount;

            if (offset > docCount)
                offset = 1;

            var documents = await _documentRepository.ListDocumentsByNomination(_nomination, offset - 1);

            var document = documents.First();

            await using FileStream fileStream = new(document.Path, FileMode.Open, FileAccess.Read, FileShare.Read);

            var leftButtonOffset = (offset - 1);
            if (leftButtonOffset <= 0)
                leftButtonOffset = docCount;

            var rightButtonOffset = (offset + 1);
            if (rightButtonOffset > docCount)
                rightButtonOffset = 1;

            var dynamicKeyboardMarkup = await new Keyboard().GetDynamicVotesKeyboard(
                leftButtonOffset, rightButtonOffset, _nomination);

            await botClient.EditMessageMediaAsync(
                chatId: callbackQuery.Message.Chat.Id, messageId: callbackQuery.Message.MessageId,
                media: new InputMediaPhoto(new InputMedia(fileStream, document.DocumentType)),
                replyMarkup: dynamicKeyboardMarkup, cancellationToken: cancellationToken
            );

            fileStream.Close();
            
            await botClient.EditMessageCaptionAsync(
                chatId: callbackQuery.Message.Chat.Id, messageId: callbackQuery.Message.MessageId,
                caption: offset + " " + document.DocumentNomination, replyMarkup: dynamicKeyboardMarkup,
                cancellationToken: cancellationToken
            );
        }
    }
    
    

    async Task ChooseNomination(
        ITelegramBotClient botClient,
        CallbackQuery? callbackQuery,
        CancellationToken cancellationToken,
        UserModel user)
    {
        await _messageService.DeleteMessages(botClient, user);

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
        await _messageService.DeleteMessages(botClient, user);

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
            await _messageService.MarkMessageToDelete(
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id, text: "Используй вирутальные кнопки", cancellationToken: cancellationToken
                )
            );

            await Task.Delay(1000);
            
            await _messageService.DeleteMessages(botClient, user);
            
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