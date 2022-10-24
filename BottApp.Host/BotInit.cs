using BottApp.Data.User;
using BottApp.Data;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using BottApp.Host.Controllers;
using Telegram.Bot.Examples.Polling;

namespace BottApp.Host
{
    public class BotInit
    {

        public void initReceiver(string token)
        {

            var bot = new TelegramBotClient(token);
            using var cts = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new UpdateType[]
                { }
            };

            bot.StartReceiving
               (Handlers.HandleUpdateAsync,
                Handlers.HandleErrorAsync,
                receiverOptions,
                cts.Token);

            Console.WriteLine($"Bot {bot.BotId} is running");
            Console.ReadLine();

            // Send cancellation request to stop bot
            cts.Cancel();
            Console.WriteLine($"Bot {bot.BotId} stopped");
        }
    }
}
