using BottApp.Database;
using BottApp.Host.Configs;
using BottApp.Host.Services;
using BottApp.Host.Services.Handlers;
using BottApp.Host.SimpleStateMachine;
using BottApp.Host.StateMachine;
using Microsoft.EntityFrameworkCore;
using Stateless;
using Telegram.Bot;
using Telegram.Bot.Extensions.StateMachine;
using BottApp.Host.StateMachine;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Telegram.Bot.Polling;

namespace BottApp.Host;

internal class TelegramBotStartup
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
        services.AddScoped<MainMenuHandler>();
        services.AddScoped<VotesHandler>();
        services.AddScoped<AuthHandler>();
        services.AddScoped<AdminChatHandler>();

    }
}