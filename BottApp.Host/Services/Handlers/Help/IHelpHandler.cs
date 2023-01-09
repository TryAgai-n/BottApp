using BottApp.Database.User;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BottApp.Host.Services.Handlers.Help;

public interface IHelpHandler : IHandler
{
    Task OnStart(ITelegramBotClient botClient, Message message);
    
}