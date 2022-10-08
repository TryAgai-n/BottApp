using Telegram.Bot;
using Telegram.Bot.Types;

namespace BottApp.Client.Bot;

public sealed class BotUpdate
{
    public ITelegramBotClient TelegramgBotClient { get; set; }

    public Update Update { get; set; }

    public CancellationToken CLToken { get; set; }


    public class Response : AbstractResponse
    {
        
    }
}