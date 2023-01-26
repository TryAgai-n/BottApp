using Telegram.Bot;
using Telegram.Bot.Types;

namespace BottApp.Host.Handlers.Help;

public interface IHelpHandler : IHandler
{
    Task OnStart(ITelegramBotClient botClient, Message message);
    
}