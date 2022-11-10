using BottApp.Database;
using BottApp.Host.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Services;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace BottApp.Host.Services.Handlers;


public class VotesUpdate : IUpdateHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<MainMenuHandler> _logger;
    public readonly IDatabaseContainer _databaseContainer;

    public VotesUpdate(ITelegramBotClient botClient, ILogger<MainMenuHandler> logger, IDatabaseContainer databaseContainer)
    {
        _botClient = botClient;
        _logger = logger;
        _databaseContainer = databaseContainer;

    }

    public async Task HandleUpdateAsync(ITelegramBotClient _, Update update, CancellationToken cancellationToken)
    {
        var handler = update switch
        {
            // UpdateType.Unknown:
            // UpdateType.ChannelPost:
            // UpdateType.EditedChannelPost:
            // UpdateType.ShippingQuery:
            // UpdateType.PreCheckoutQuery:
            // UpdateType.Poll:
            { Message: { } message }                       => BotOnMessageReceived(message, cancellationToken),
            { EditedMessage: { } message }                 => BotOnMessageReceived(message, cancellationToken),
            { CallbackQuery: { } callbackQuery }           => BotOnCallbackQueryReceived(_,callbackQuery, cancellationToken),
           // { InlineQuery: { } inlineQuery }               => BotOnInlineQueryReceived(inlineQuery, cancellationToken),
            // { ChosenInlineResult: { } chosenInlineResult } => BotOnChosenInlineResultReceived(chosenInlineResult, cancellationToken),
            _                                              => UnknownUpdateHandlerAsync(update, cancellationToken)
        };

        await handler;
    }



    #region Inline Mode
    
    public static string GetTimeEmooji()
    {
        string[] emooji = {"🕐","🕑","🕒","🕓","🕔","🕕","🕖","🕗","🕘","🕙","🕚","🕛","🕐 ","🕑 ",};
        var rand = new Random();
        var preparedString = emooji[rand.Next(0, emooji.Length)];
        return preparedString;
       
    }

    public static async Task<Message> TryEditMessage(ITelegramBotClient? botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
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
                    replyMarkup: Keyboard.MainKeyboardMarkup,
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
                    replyMarkup: Keyboard.MainKeyboardMarkup,
                    cancellationToken: cancellationToken
                );
            }
        }
        catch (Exception e)
        {
            return await botClient.SendTextMessageAsync
            (
            chatId: callbackQuery.Message.Chat.Id,
            text: viewExceptionText+"\n"+e,
            replyMarkup: Keyboard.MainKeyboardMarkup,
            cancellationToken: cancellationToken
            );
        }
    }

    public async Task BotOnCallbackQueryReceived(ITelegramBotClient? botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received inline keyboard callback from: {CallbackQueryId}", callbackQuery.Id);
        await MessageManager.SaveInlineMessage(_databaseContainer, callbackQuery);
        var guid = Guid.NewGuid().ToString("N");
        
        var action = callbackQuery.Data.Split(' ')[0] switch
        {
      
            "ButtonVotes"          => await SendInlineVotesKeyboard(botClient,callbackQuery,cancellationToken),
            "ButtonRequestContact" => await InlineRequestContactAndLocation(botClient,callbackQuery,cancellationToken),
            "ButtonBack"           =>  await botClient.SendTextMessageAsync(
                chatId: callbackQuery.Message.Chat.Id,
                text: "Главное меню",
                replyMarkup: Keyboard.MainKeyboardMarkup,
                cancellationToken: cancellationToken),
            
            _                      =>  await TryEditMessage(botClient, callbackQuery, cancellationToken)
       
        };

        async Task<Message> SendInlineVotesKeyboard(ITelegramBotClient botClient,CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            await botClient.SendChatActionAsync(
                chatId: callbackQuery.Message.Chat.Id,
                chatAction: ChatAction.Typing,
                cancellationToken: cancellationToken);

            // Simulate longer running task
            await Task.Delay(500, cancellationToken);
            
             await botClient.SendTextMessageAsync(
                chatId: callbackQuery.Message.Chat.Id,
                text: "Голосование",
                cancellationToken: cancellationToken);

            return await DocumentManager.SendVotesDocument(_databaseContainer, callbackQuery, botClient, cancellationToken);
        }
        
         async Task<Message> InlineRequestContactAndLocation(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
             await botClient.SendTextMessageAsync(
                chatId: callbackQuery.Message.Chat.Id,
                text: "Привет! необходим твой номер телефона, чтобы я могли идентифицировать тебя.",
                cancellationToken: cancellationToken);
             
             await Task.Delay(750);
             
             await botClient.SendTextMessageAsync(
                 chatId: callbackQuery.Message.Chat.Id,
                 text: "Не переживай! Твои данные не передаются третьим лицам и хранятся на безопасном сервере",
                 cancellationToken: cancellationToken);
             
             await Task.Delay(1500);

             return await botClient.SendTextMessageAsync(
                 chatId: callbackQuery.Message.Chat.Id,
                 text: "Нажми на кнопку 'Поделиться номером' ниже",
                 replyMarkup: Keyboard.RequestLocationAndContactKeyboard,
                 cancellationToken: cancellationToken);
        }
    }

    #endregion
    
    #region Simple Message Mode
    private async Task BotOnMessageReceived(Message message, CancellationToken cancellationToken)
    {
        
        await MessageManager.SaveMessage(_databaseContainer, message);
        
        if (message.Contact != null)
            await UserManager.UpdateContact(message, _botClient, cancellationToken,_databaseContainer);
        
        if (await UserManager.UserPhoneHasOnDb(_databaseContainer, message) == false)
        {
           await RequestContactAndLocation(_botClient, message, cancellationToken);
           return;
        }
        
        
        static async Task<Message> RequestContactAndLocation(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Привет! Мне необходим твой номер телефона, чтобы я мог идентифицировать тебя.",
                cancellationToken: cancellationToken);
             
            await Task.Delay(750);
             
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Не переживай! Твои данные не передаются третьим лицам и хранятся на безопасном сервере =)",
                cancellationToken: cancellationToken);
             
            await Task.Delay(1500);

            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Нажми на кнопку 'Поделиться контактом' ниже",
                replyMarkup: Keyboard.RequestLocationAndContactKeyboard,
                cancellationToken: cancellationToken);
        }
        
        _logger.LogInformation("Receive message type: {MessageType}", message.Type);
        if (message.Text is not { } messageText)
            return;

        var action = messageText.Split(' ')[0] switch
        {
            "/votes"           => SendInlineVotesKeyboard(_botClient, message, cancellationToken),
            "/keyboard"        => SendReplyKeyboard(_botClient, message, cancellationToken),
            "/remove"          => RemoveKeyboard(_botClient, message, cancellationToken),
            "/photo"           => SendFile(_botClient, message, cancellationToken),
            "/request"         => RequestContactAndLocation(_botClient, message, cancellationToken),
            "/inline_mode"     => StartInlineQuery(_botClient, message, cancellationToken),
            "/throw"           => FailingHandler(_botClient, message, cancellationToken),
            _                  => Usage(_botClient, message, cancellationToken)
        };
        
        Message sentMessage = await action;
        _logger.LogInformation("The message was sent with id: {SentMessageId}", sentMessage.MessageId);
        
        static async Task<Message> SendInlineVotesKeyboard(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            await botClient.SendChatActionAsync(
                chatId: message.Chat.Id,
                chatAction: ChatAction.Typing,
                cancellationToken: cancellationToken);

            // Simulate longer running task
            await Task.Delay(500, cancellationToken);
            
            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Голосование",
                replyMarkup: Keyboard.VotesKeyboardMarkup,
                cancellationToken: cancellationToken);
        }

        static async Task<Message> SendReplyKeyboard(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            ReplyKeyboardMarkup replyKeyboardMarkup = new(
                new[]
                {
                        new KeyboardButton[] { "1.1", "1.2" },
                        new KeyboardButton[] { "2.1", "2.2" },
                })
            {
                ResizeKeyboard = true
            };

            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Choose",
                replyMarkup: replyKeyboardMarkup,
                cancellationToken: cancellationToken);
        }

        static async Task<Message> RemoveKeyboard(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Removing keyboard",
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: cancellationToken);
        }

        static async Task<Message> SendFile(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            await botClient.SendChatActionAsync(
                message.Chat.Id,
                ChatAction.UploadPhoto,
                cancellationToken: cancellationToken);

            const string filePath = @"Files/tux.png";
            await using FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var fileName = filePath.Split(Path.DirectorySeparatorChar).Last();

            return await botClient.SendPhotoAsync(
                chatId: message.Chat.Id,
                photo: new InputOnlineFile(fileStream, fileName),
                caption: "Nice Picture",
                cancellationToken: cancellationToken);
        }

        static async Task<Message> Usage(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Главное Меню",
                replyMarkup: Keyboard.MainKeyboardMarkup,
                cancellationToken: cancellationToken);
        }

        static async Task<Message> StartInlineQuery(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            InlineKeyboardMarkup inlineKeyboard = new(
                InlineKeyboardButton.WithSwitchInlineQueryCurrentChat("Inline Mode"));

            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Press the button to start Inline Query",
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }

        static Task<Message> FailingHandler(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            throw new IndexOutOfRangeException();
        }
    }
    
    #endregion

    #region Other methods
    
    private Task UnknownUpdateHandlerAsync(Update update, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
        return Task.CompletedTask;
    }

    public async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogInformation("HandleError: {ErrorMessage}", ErrorMessage);

        // Cooldown in case of network connection error
        if (exception is RequestException)
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    }
    #endregion
}
