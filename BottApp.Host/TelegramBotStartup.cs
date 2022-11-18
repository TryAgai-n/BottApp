using BottApp.Host.Configs;
using BottApp.Host.Services;
using BottApp.Host.Services.Handlers;
using BottApp.Host.SimpleStateMachine;
using Telegram.Bot;
using Telegram.Bot.Polling;

namespace BottApp.Host;

internal static class TelegramBotStartup
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<BotConfig>(configuration.GetSection("BotConfiguration"));

        var botConfig = configuration.GetSection("BotConfiguration").Get<BotConfig>();

        services.AddHttpClient("telegram_bot_client")
            .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
            {
                // BotConfig? botConfig = sp.GetConfiguration<BotConfig>();
                TelegramBotClientOptions options = new(botConfig.Token);
                return new TelegramBotClient(options, httpClient);
            });

        services.AddHostedService<PollingService>();
        services.AddScoped<ReceiverService>();
        
        services.AddScoped<IUpdateHandler, UpdateHandler>();
        services.AddScoped<SimpleFSM>();
    }
}