using BottApp.Database;
using BottApp.Database.Service;
using BottApp.Host;
using BottApp.Host.Controllers;
using BottApp.Host.Handlers;
using BottApp.Host.Services;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Factory = BottApp.Host.Handlers.Factory;

var builder = WebApplication.CreateBuilder(args);

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

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

AdminSettings.AdminChatId = Convert.ToInt64(builder.Configuration.GetSection("AdminSettings:AdminChatId").Value);

builder.Services.AddScoped<UpdateHandlers>();
builder.Services.AddScoped<UpdateHandler>();
builder.Services.AddHostedService<ConfigureWebhook>();

builder.Services
    .AddControllers()
    .AddNewtonsoftJson();

var typeOfContent = typeof(Program);

builder.Services.AddDbContext<PostgreSqlContext>(
    opt => opt.UseNpgsql(
        builder.Configuration.GetConnectionString("PostgreSqlConnection"),
        b => b.MigrationsAssembly(typeOfContent.Assembly.GetName().Name)));

builder.Services.AddScoped<IServiceContainer>(
    x => BottApp.Database.Service.Factory.Create
    (
        x.GetRequiredService<IDatabaseContainer>())
);
        
builder.Services.AddScoped<IDatabaseContainer, DatabaseContainer>();
builder.Services.AddScoped<IHandlerContainer>(
    x => Factory.Create
    (
        x.GetRequiredService<IDatabaseContainer>(),
        x.GetRequiredService<IServiceContainer>()
    )
);

var app = builder.Build();

app.MapBotWebhookRoute<BotController>(route: botConfiguration.Route);
app.MapControllers();
app.Run();