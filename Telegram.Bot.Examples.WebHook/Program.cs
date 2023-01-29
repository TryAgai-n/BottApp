using BottApp.Database;
using BottApp.Host.Handlers;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Examples.WebHook;
using Telegram.Bot.Examples.WebHook.Controllers;
using Telegram.Bot.Examples.WebHook.Services;

var builder = WebApplication.CreateBuilder(args);

var botConfigurationSection = builder.Configuration.GetSection(BotConfiguration.Configuration);
builder.Services.Configure<BotConfiguration>(botConfigurationSection);

var botConfiguration = botConfigurationSection.Get<BotConfiguration>();
builder.Services.AddHttpClient("telegram_bot_client")
                .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
                {
                    var botConfig = sp.GetConfiguration<BotConfiguration>();
                    TelegramBotClientOptions options = new(botConfig.BotToken);
                    return new TelegramBotClient(options, httpClient);
                });

builder.Services.AddScoped<UpdateHandlers>();
builder.Services.AddScoped<UpdateHandler>();
// builder.Services.AddSingleton<TunnelService>();
builder.Services.AddHostedService<ConfigureWebhook>();

builder.Services
    .AddControllers()
    .AddNewtonsoftJson();


builder.Services.AddDbContext<PostgreSqlContext>(
    opt => opt.UseNpgsql(
        builder.Configuration.GetConnectionString("PostgreSqlConnection")));
        
builder.Services.AddScoped<BottApp.Database.Service.IServiceContainer>(x => BottApp.Database.Service.Factory.Create(x.GetRequiredService<IDatabaseContainer>()));
        
builder.Services.AddScoped<IDatabaseContainer, DatabaseContainer>();
builder.Services.AddScoped<IHandlerContainer>(
    x => Factory.Create
    (
        x.GetRequiredService<IDatabaseContainer>(),
        x.GetRequiredService<BottApp.Database.Service.IServiceContainer>()
    )
);

var app = builder.Build();

app.MapBotWebhookRoute<BotController>(route: botConfiguration.Route);
app.MapControllers();
app.Run();