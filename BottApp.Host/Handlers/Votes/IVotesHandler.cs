using Telegram.Bot;
using Telegram.Bot.Types;

namespace BottApp.Host.Handlers.Votes;

public interface IVotesHandler : IHandler
{
    Task OnStart(ITelegramBotClient botClient, Message message);
}