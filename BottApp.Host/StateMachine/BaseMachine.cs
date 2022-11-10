namespace BottApp.Host.StateMachine;
using Stateless;


public class BaseMachine : IBaseMachine
{
    public static void MachineConfig(StateMachine<MState, MAction> bot)
    {
        // var bot = new StateMachine<MState, MAction>(MState.MainMenu);
        Console.WriteLine($"Start_State: {bot.State} \n"); // Точка входа - Auth

        bot.Configure(MState.Auth)
            .OnEntry(s => Console.WriteLine($"Entry to {s.Destination} in {s.Source}"))
            .OnExit(s => Console.WriteLine($"Exit in {s.Source}, Destination {s.Destination}"))
            .PermitReentry(MAction.GetAuth)
            .Permit(MAction.GetMainMenu, MState.MainMenu);

        bot.Configure(MState.MainMenu)
            .OnEntry(s => Console.WriteLine($"Entry to {s.Destination} in {s.Source}"))
            .OnExit(s => Console.WriteLine($"Exit in {s.Source}, Destination {s.Destination}"))
            .Permit(MAction.GetVotes, MState.Votes)
            .Permit(MAction.GetHelp, MState.Help);
            
            
        bot.Configure(MState.Votes)
            .OnEntryAsync
            (s => 
                { 
                    Console.WriteLine($"Entry ASYNC to {s.Destination} in {s.Source}");
                    return Votes.RunVotes(bot, true); 
                }
            )
                
            .OnExit(s => Console.WriteLine($"Exit in {s.Source}, Destination {s.Destination}"))
            .Permit(MAction.GetMainMenu, MState.MainMenu)
            .Permit(MAction.GetVotesDB, MState.OnVotesDB);


        // string graph = UmlDotGraph.Format(bot.GetInfo());
        // Console.WriteLine("GPAPH||||||||||  \n" + graph + "\n||||||||||");
        
    }
}