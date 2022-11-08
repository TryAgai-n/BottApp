using Telegram.Bot;
using Telegram.Bot.Types;

namespace BottApp.Client.Bot;

public sealed class BotUpdate
{
    public ITelegramBotClient TelegramgBotClient { get; set; }

    public Update Update { get; set; }
    
    public Message Message { get; set; }

    public CancellationToken CLToken { get; set; }


    public class Response : AbstractResponse
    {
        public Response()
        {
            Console.WriteLine("Ты создан! А значит, я Бог!");
        }
    }
}