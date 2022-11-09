using BottApp.Host.Exp.Configs;
using Telegram.Bot;

namespace BottApp.Host.Exp;

internal class TelegramBotStartup
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<BotConfig>(
            configuration.GetSection(BotConfig.Configuration));


        services.AddHttpClient("telegram_bot_client")
            .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
            {
                BotConfig? botConfig = sp.GetConfiguration<BotConfig>();
                TelegramBotClientOptions options = new(botConfig.BotToken);
                return new TelegramBotClient(options, httpClient);
            });
    }
}