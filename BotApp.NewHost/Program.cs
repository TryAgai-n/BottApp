using BottApp.Database;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Services;

namespace BottApp.Host;

class Program
{
    public static async Task Main(string[] args)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        IHost host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Register Bot configuration
                services.Configure<BotConfiguration>(
                    context.Configuration.GetSection(BotConfiguration.Configuration));


                services.AddHttpClient("telegram_bot_client")
                    .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
                    {
                        BotConfiguration? botConfig = sp.GetConfiguration<BotConfiguration>();
                        TelegramBotClientOptions options = new(botConfig.BotToken);
                        return new TelegramBotClient(options, httpClient);
                    });
                // Register DB configuration

                services.AddDbContext<PostgreSqlContext>(options =>
                    options.UseNpgsql(context.Configuration.GetConnectionString("PostgreSqlConnection")));
        
                services.AddScoped<IDatabaseContainer, DatabaseContainer>();
       
                services.AddScoped<UpdateHandler>();
                services.AddScoped<ReceiverService>();
                services.AddHostedService<PollingService>();
            })
            .Build();

        await host.RunAsync();
    }
}

public class BotConfiguration
{
    public static readonly string Configuration = "BotConfiguration";
    public string BotToken { get; set; } = "";
}