using BottApp.Database;
using BottApp.Host.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace BottApp.Host.Services;

public static class UserManager
{
    public static async Task Save(IDatabaseContainer _databaseContainer, Message message)
    {
        var user = await _databaseContainer.User.FindOneById((int) message.Chat.Id);
        if (user == null)
        {
            await _databaseContainer.User.CreateUser(message.Chat.Id, message.Chat.FirstName, null);
        }
    }

    public static async Task<bool> UserPhoneHasOnDb(IDatabaseContainer _databaseContainer, Message message)
    {
        var user = await _databaseContainer.User.FindOneById((int) message.Chat.Id);
        if (user.Phone == null)
             return false;
        
        return true;
    }
    
    public static async Task UpdateContact(Message message, ITelegramBotClient botClient, CancellationToken cancellationToken, IDatabaseContainer _databaseContainer)
    {
        var user = await _databaseContainer.User.FindOneById((int)message.Chat.Id);

        if (user.Phone == null)
        {
            await _databaseContainer.User.UpdateUserPhone(user,  message.Contact.PhoneNumber);
            
            var action = await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "...обработка",
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: cancellationToken);

            await  botClient.DeleteMessageAsync(
                chatId: message.Chat.Id,
                messageId: action.MessageId,
                cancellationToken: cancellationToken);
           
            await Task.Delay(1000);
             
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Вы авторизованы!",
                replyMarkup: Keyboard.MainKeyboardMarkup,
                cancellationToken: cancellationToken);
        } else {
            
           var action = await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "...обработка",
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: cancellationToken);
           
           await Task.Delay(1000);
           
           await  botClient.DeleteMessageAsync(
               chatId: message.Chat.Id,
               messageId: action.MessageId,
               cancellationToken: cancellationToken);
           
           await Task.Delay(1000);
           
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Вы уже отправляли контакт, спасибо!",
                replyMarkup: Keyboard.MainKeyboardMarkup,
                cancellationToken: cancellationToken);
        }
    }
}