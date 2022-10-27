using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Examples.Polling;

namespace BottApp.Host
{
    public static class BotInit
    {
        public static void InitReceiver(Configs.BotConfig config)
        {
            var bot = new TelegramBotClient(config.Token);
            using var cts = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new UpdateType[]
                { }
            };
            //Подписываемся на 
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
