using BottApp.Database;
using BottApp.Host.Keyboards;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Services;

public static class MessageManager
{
    public static async Task SaveMessage(IDatabaseContainer _databaseContainer, Message? message)
    {
        var user = await _databaseContainer.User.FindOneByUid((int)message.Chat.Id);
        string type = message.Type.ToString();
        await _databaseContainer.Message.CreateModel(user.Id, message.Text, type, DateTime.Now);
    }

    public static async Task SaveInlineMessage(IDatabaseContainer _databaseContainer, CallbackQuery callbackQuery)
    {
        var user = await _databaseContainer.User.FindOneByUid((int)callbackQuery.Message.Chat.Id);
        string type = callbackQuery.GetType().ToString();
        await _databaseContainer.Message.CreateModel(user.Id, callbackQuery.Data, type, DateTime.Now);
    }

    public static async Task<Message> TryEditInlineMessage
    (ITelegramBotClient? botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken, Keyboard keyboard)
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


    public static string GetTimeEmooji()
    {
        string[] emooji = { "🕐", "🕑", "🕒", "🕓", "🕔", "🕕", "🕖", "🕗", "🕘", "🕙", "🕚", "🕛", "🕐 ", "🕑 ", };
        var rand = new Random();
        var preparedString = emooji[rand.Next(0, emooji.Length)];
        return preparedString;
    }
}