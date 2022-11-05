using BottApp.Database;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Services;

public static class MessageManager
{
    public static async Task Save(IDatabaseContainer _databaseContainer, Message message)
    {
        var user = await _databaseContainer.User.FindOneById((int) message.Chat.Id);
        string type = message.Type.ToString();
        await _databaseContainer.Message.CreateModel(user.Id, message.Text, type, DateTime.Now);
    }
    
    public static async Task UpdateContact(Message message, ITelegramBotClient botClient, CancellationToken cancellationToken, IDatabaseContainer _databaseContainer)
    {
        var user = await _databaseContainer.User.FindOneById((int)message.Chat.Id);

        if (user.Phone == null)
        {
            await _databaseContainer.User.UpdateUserPhone(user,  message.Contact.PhoneNumber);
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Записал в базу!",
                replyMarkup: new  ReplyKeyboardRemove(),
                cancellationToken: cancellationToken);
        } else {
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Ты уже есть в базе!",
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: cancellationToken);
        }
    }
}