using BottApp.Database;
using BottApp.Host.Keyboards;
using BottApp.Host.SimpleStateMachine;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BottApp.Host.Services.Handlers.Votes;

public class VotesHandler : IVotesHandler
{
    
    #region Inline Mode

    public string GetTimeEmooji()
    {
        string[] emooji = {"🕐", "🕑", "🕒", "🕓", "🕔", "🕕", "🕖", "🕗", "🕘", "🕙", "🕚", "🕛", "🕐 ", "🕑 ",};
        var rand = new Random();
        var preparedString = emooji[rand.Next(0, emooji.Length)];
        return preparedString;
    }

    public async Task<Message> TryEditMessage
        (ITelegramBotClient? botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
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
                text: viewExceptionText + "\n" + e,
                replyMarkup: Keyboard.MainKeyboardMarkup,
                cancellationToken: cancellationToken
            );
        }
    }

    public async Task BotOnCallbackQueryReceived(SimpleFSM FSM, ITelegramBotClient? botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        // _logger.LogInformation("Received inline keyboard callback from: {CallbackQueryId}", callbackQuery.Id);
        var guid = Guid.NewGuid().ToString("N");

        if (callbackQuery.Data == "ButtonVotes")
        {
            FSM.SetState(UserState.Votes);
            await SendInlineVotesKeyboard(botClient, callbackQuery, cancellationToken);
            return;
        }
        
        var action = callbackQuery.Data.Split(' ')[0] switch
        {
            "ButtonRight" => await DocumentManager.SendVotesDocument
                (callbackQuery, botClient, cancellationToken),
            "ButtonLeft" => await DocumentManager.SendVotesDocument
                (callbackQuery, botClient, cancellationToken),
            "ButtonRequestContact" => await InlineRequestContactAndLocation
                (botClient, callbackQuery, cancellationToken),
            "ButtonBack" => await botClient.SendTextMessageAsync
            (
                chatId: callbackQuery.Message.Chat.Id,
                text: "Главное меню",
                replyMarkup: Keyboard.MainKeyboardMarkup,
                cancellationToken: cancellationToken
            ),

            _ => await TryEditMessage(botClient, callbackQuery, cancellationToken)
        };


        async Task<Message> SendInlineVotesKeyboard
            (ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            await botClient.SendChatActionAsync
            (
                chatId: callbackQuery.Message.Chat.Id,
                chatAction: ChatAction.Typing,
                cancellationToken: cancellationToken
            );

            // Simulate longer running task
            await Task.Delay(500, cancellationToken);

            await botClient.SendTextMessageAsync
            (
                chatId: callbackQuery.Message.Chat.Id,
                text: "Голосование",
                cancellationToken: cancellationToken
            );

            return await DocumentManager.SendVotesDocument
                (callbackQuery, botClient, cancellationToken);
        }

        async Task<Message> InlineRequestContactAndLocation
            (ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            await botClient.SendTextMessageAsync
            (
                chatId: callbackQuery.Message.Chat.Id,
                text: "Привет! необходим твой номер телефона, чтобы я могли идентифицировать тебя.",
                cancellationToken: cancellationToken
            );

            await Task.Delay(750);

            await botClient.SendTextMessageAsync
            (
                chatId: callbackQuery.Message.Chat.Id,
                text: "Не переживай! Твои данные не передаются третьим лицам и хранятся на безопасном сервере",
                cancellationToken: cancellationToken
            );

            await Task.Delay(1500);

            return await botClient.SendTextMessageAsync
            (
                chatId: callbackQuery.Message.Chat.Id,
                text: "Нажми на кнопку 'Поделиться номером' ниже",
                replyMarkup: Keyboard.RequestLocationAndContactKeyboard,
                cancellationToken: cancellationToken
            );
        }
    }

    #endregion

    #region Simple Message Mode

    public async Task BotOnMessageReceivedVotes
        (ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        await botClient.SendTextMessageAsync
        (
            chatId: message.Chat.Id,
            text: "Ты в вотсе",
            cancellationToken: cancellationToken
        );
    }

    public async Task BotOnCallbackQueryReceivedVotes
        (ITelegramBotClient? botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        // _logger.LogInformation("Received inline keyboard callback from: {CallbackQueryId}", callbackQuery.Id);
    }

    public async Task BotOnMessageReceived(SimpleFSM FSM,ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        // await MessageManager.SaveMessage(_dbContainer, message);
        //
        // if (message.Contact != null)
        //     await UserManager.UpdateContact(message, botClient, cancellationToken, _dbContainer);
        //
        // if (await UserManager.UserPhoneHasOnDb(_dbContainer, message) == false)
        // {
        //     await RequestContactAndLocation(botClient, message, cancellationToken);
        //     return;
        // }

        // _logger.LogInformation("Receive message type: {MessageType}", message.Type);
        if (message.Text is not { } messageText)
            return;

        var action = messageText.Split(' ')[0] switch
        {
            _ => Usage(botClient, message, cancellationToken)
        };
        

        static async Task<Message> Usage(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            return await botClient.SendTextMessageAsync
            (
                chatId: message.Chat.Id,
                text: "Главное Меню",
                replyMarkup: Keyboard.MainKeyboardMarkup,
                cancellationToken: cancellationToken
            );
        }
    }

    #endregion

    #region Other methods

    public Task UnknownUpdateHandlerAsync(Update update, CancellationToken cancellationToken)
    {
        // _logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
        return Task.CompletedTask;
    }

    public async Task HandlePollingErrorAsync
        (ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
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

    #endregion
}