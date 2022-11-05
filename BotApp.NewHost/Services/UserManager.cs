using BottApp.Database;
using Telegram.Bot.Types;

namespace Telegram.Bot.Services;

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
}