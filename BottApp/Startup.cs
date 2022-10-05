using System;
using Telegram.Bot;


namespace BottApp
{
    public class Startup
    {
        public static void Main(string[] args)
        {
            var token = GetConfig.GetBotToken();
            var bot = new TelegramBotClient(token);
            Program.initReceiver(bot);

            Console.ReadLine();
        }
    }
}