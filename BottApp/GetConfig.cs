using BottApp.Configs;
using Microsoft.Extensions.Configuration;

namespace BottApp
{
    internal class GetConfig
    {
        public static string GetByName(string configKeyName)
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            var section = config.GetSection(configKeyName);

            return section.Value;

        }

        public static string GetBotToken()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json").Build();

            var section = config.GetSection(nameof(BotConfig));
            var botConfig = section.Get<BotConfig>();

            return botConfig.Token;

        }
    }
}
