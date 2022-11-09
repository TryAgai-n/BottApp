using BottApp.Database;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Services;

public static class MessageManager
{
    public static async Task SaveMessage(IDatabaseContainer _databaseContainer, Message message)
    {
        var user = await _databaseContainer.User.FindOneById((int) message.Chat.Id);
        string type = message.Type.ToString();
        await _databaseContainer.Message.CreateModel(user.Id, message.Text, type, DateTime.Now);
    }
    
    public static async Task SaveInlineMessage(IDatabaseContainer _databaseContainer, CallbackQuery callbackQuery)
    {
        var user = await _databaseContainer.User.FindOneById((int) callbackQuery.Message.Chat.Id);
        string type = callbackQuery.GetType().ToString();
        await _databaseContainer.Message.CreateModel(user.Id, callbackQuery.Data, type, DateTime.Now);
    }
    
  
}