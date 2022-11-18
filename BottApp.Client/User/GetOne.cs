using System.ComponentModel.DataAnnotations;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BottApp.Client.User;

public sealed class GetOne
{
    [Required]
    public ITelegramBotClient TelegramBotClient { get; }

    [Required]
    public Update Update { get; }

    [Required]
    public CancellationToken CancellationToken { get; }
    
    
    public sealed class Response : AbstractResponse
    {
        public Payload.User.User User { get; }


        public Response(Payload.User.User user)
        {
            User = user;
        }
    }
}