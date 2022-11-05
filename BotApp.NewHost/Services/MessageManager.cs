using BottApp.Database;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Services;

public static class MessageManager
{
    public static async Task Save(IDatabaseContainer _databaseContainer, Message message)
    {
        var user = await _databaseContainer.User.FindOneById((int) message.Chat.Id);
        await _databaseContainer.Message.CreateModel(user.Id, message.Text, DateTime.Now);
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