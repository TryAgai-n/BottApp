using BottApp.Database.User;
using BottApp.Host.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace BottApp.Host.Services.Handlers.MainMenu;

public class MainMenuHandler : IMainMenuHandler
{
    private readonly IUserRepository _userRepository;

    private readonly DocumentManager _documentManager;

    public MainMenuHandler(IUserRepository userRepository, DocumentManager documentManager)
    {
        _userRepository = userRepository;
        _documentManager = documentManager;
    }


    public string GetTimeEmooji()
    {
        string[] emooji = {"🕐", "🕑", "🕒", "🕓", "🕔", "🕕", "🕖", "🕗", "🕘", "🕙", "🕚", "🕛", "🕐 ", "🕑 ",};
        var rand = new Random();
        var preparedString = emooji[rand.Next(0, emooji.Length)];
        return preparedString;
    }


    public async Task<Message> TryEditMessage(
        ITelegramBotClient? botClient,
        CallbackQuery callbackQuery,
        CancellationToken cancellationToken
    )
    {
        var viewText = "Такой команды еще нет ";
        var viewExceptionText = "Все сломаделось : ";

        var editText = viewText + GetTimeEmooji();

        try
        {
            try
            {
                return await botClient.EditMessageTextAsync(
                    chatId: callbackQuery.Message.Chat.Id, messageId: callbackQuery.Message.MessageId, text: editText,
                    replyMarkup: Keyboard.MainKeyboardMarkup, cancellationToken: cancellationToken
                );
            }
            catch
            {
                editText = viewText + GetTimeEmooji();

                return await botClient.EditMessageTextAsync(
                    chatId: callbackQuery.Message.Chat.Id, messageId: callbackQuery.Message.MessageId, text: editText,
                    replyMarkup: Keyboard.MainKeyboardMarkup, cancellationToken: cancellationToken
                );
            }
        }
        catch (Exception e)
        {
            return await botClient.SendTextMessageAsync(
                chatId: callbackQuery.Message.Chat.Id, text: viewExceptionText + "\n" + e,
                replyMarkup: Keyboard.MainKeyboardMarkup, cancellationToken: cancellationToken
            );
        }
    }


    public async Task BotOnCallbackQueryReceived(
        ITelegramBotClient? botClient,
        CallbackQuery callbackQuery,
        CancellationToken cancellationToken
    )
    {
        // _logger.LogInformation("Received inline keyboard callback from: {CallbackQueryId}", callbackQuery.Id);
        // await MessageManager.SaveInlineMessage(_dbContainer, callbackQuery);

        var user = await _userRepository.GetOneByUid(callbackQuery.Message.Chat.Id);
        
        if (callbackQuery.Data == "ButtonVotes")
        {
            await _userRepository.ChangeOnState(user, OnState.Votes);
            await SendInlineVotesKeyboard(botClient, callbackQuery, cancellationToken);
            return;
        }

        var action = callbackQuery.Data.Split(' ')[0] switch
        {
            "ButtonHi" => await botClient.SendTextMessageAsync(
                chatId: -1001824488986, text: callbackQuery.Message.Chat.FirstName + " говорит привет!",
                cancellationToken: cancellationToken
            ),
            "ButtonBack" => await botClient.SendTextMessageAsync(
                chatId: callbackQuery.Message.Chat.Id, text: "Главное меню", replyMarkup: Keyboard.MainKeyboardMarkup,
                cancellationToken: cancellationToken
            ),

            _ => await TryEditMessage(botClient, callbackQuery, cancellationToken)
        };
        
        async Task<Message> SendInlineVotesKeyboard(
            ITelegramBotClient botClient,
            CallbackQuery callbackQuery,
            CancellationToken cancellationToken)
        {


            await botClient.DeleteMessageAsync(
                chatId: callbackQuery.Message.Chat.Id,
                messageId: callbackQuery.Message.MessageId,
                cancellationToken: cancellationToken);
          
            
            await botClient.SendChatActionAsync(
                chatId: callbackQuery.Message.Chat.Id, chatAction: ChatAction.Typing,
                cancellationToken: cancellationToken
            );

            return await botClient.SendTextMessageAsync
            (
                chatId: callbackQuery.Message.Chat.Id, 
                text: "Меню: Голосование", 
                replyMarkup: Keyboard.MainVotesKeyboardMarkup,
                cancellationToken: cancellationToken
            );
        }
    }


    public async Task BotOnMessageReceived(
        ITelegramBotClient botClient,
        Message message,
        CancellationToken cancellationToken
    )
    {
        if (message.Document != null)
        {
           await _documentManager.UploadFile(message, botClient);
        }

        await Usage(botClient, message, cancellationToken);

        static async Task<Message> SendFile(
            ITelegramBotClient botClient,
            Message message,
            CancellationToken cancellationToken
        )
        {
            await botClient.SendChatActionAsync(
                message.Chat.Id, ChatAction.UploadPhoto, cancellationToken: cancellationToken
            );

            const string filePath = @"Files/tux.png";
            await using FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var fileName = filePath.Split(Path.DirectorySeparatorChar).Last();

            return await botClient.SendPhotoAsync(
                chatId: message.Chat.Id, photo: new InputOnlineFile(fileStream, fileName), caption: "Nice Picture",
                cancellationToken: cancellationToken
            );
        }

        static async Task<Message> Usage(
            ITelegramBotClient botClient,
            Message message,
            CancellationToken cancellationToken
        )
        {
            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id, text: "Главное Меню", replyMarkup: Keyboard.MainKeyboardMarkup,
                cancellationToken: cancellationToken
            );
        }

        static async Task<Message> StartInlineQuery(
            ITelegramBotClient botClient,
            Message message,
            CancellationToken cancellationToken
        )
        {
            InlineKeyboardMarkup inlineKeyboard =
                new(InlineKeyboardButton.WithSwitchInlineQueryCurrentChat("Inline Mode"));

            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id, text: "Press the button to start Inline Query", replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken
            );
        }

        static Task<Message> FailingHandler(
            ITelegramBotClient botClient,
            Message message,
            CancellationToken cancellationToken
        )
        {
            throw new IndexOutOfRangeException();
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

        // _logger.LogInformation("HandleError: {ErrorMessage}", ErrorMessage);

        // Cooldown in case of network connection error
        if (exception is RequestException)
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    }
}