using BottApp.Database.User;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BottApp.Host.Services.OnStateStart;

public interface IStateService
{
    Task StartState(UserModel user, OnState state, ITelegramBotClient bot, Message message);
}