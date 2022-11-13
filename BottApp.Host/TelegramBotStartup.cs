using BottApp.Database;
using BottApp.Host.Configs;
using BottApp.Host.Services;
using BottApp.Host.Services.Handlers;
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
    public static StateMachine<StateStatus, ActionStatus> FSM = new (StateStatus.Auth);
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
        
        
        
        // var serviceDescriptor = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IService));
        // services.Remove(serviceDescriptor);
        
        
      

        Console.WriteLine($"Start_State: {FSM.State} \n");


        FSM.Configure(StateStatus.Auth)
            .Permit(ActionStatus.GetMainMenu, StateStatus.MainMenu)
            .PermitReentry(ActionStatus.GetAuth);
        
        FSM.Configure(StateStatus.MainMenu)
            .OnEntry(
                s =>
                {
                    services.AddScoped<IUpdateHandler, MainMenuHandler>();
                    Console.WriteLine($"Entry to {s.Destination} in {s.Source}");
                    // services.AddScoped<IUpdateHandler, MainMenuHandler>();
                }
            )
            .OnExit(s =>
            {
                var serviceDescriptor = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IUpdateHandler));

                services.Remove(serviceDescriptor);

                if (serviceDescriptor == null)
                {
                    throw new Exception("NULL ------------------- NULL ------------------ NULL");
                }

                Console.WriteLine($"Exit from {s.Source} to {s.Destination}");
              

            })
            .Permit(ActionStatus.GetVotes, StateStatus.Votes)
            .Permit(ActionStatus.GetHelp, StateStatus.Help)
            .PermitReentry(ActionStatus.GetMainMenu);


        FSM.Configure(StateStatus.Votes)
            .OnEntry
            (
                s =>
                {

                    Console.WriteLine($"Entry to {s.Destination} in {s.Source}");
                    
                    
                    services.AddScoped<IUpdateHandler, VotesHandler>();

                    //return Votes.RunVotes(FSM, true);
                }
            )
            .OnExit
            (
                s =>
                {
                    Console.WriteLine($"Exit from {s.Source} to {s.Destination}");
                }
            )
            .Permit(ActionStatus.GetMainMenu, StateStatus.MainMenu)
            .Permit(ActionStatus.GetVotesDB, StateStatus.OnVotesDB)
            .PermitReentry(ActionStatus.GetVotes);
        
         FSM.Fire(ActionStatus.GetMainMenu);
            
    }
}