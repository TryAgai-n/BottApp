using Stateless;
using Stateless.Graph;
using StateMachine.Bot;

namespace StateMachine
{
    public class Program
    { 
        public static void Main(string[] args)
        {
            bool userIsAuth = false;
            
            var bot = new StateMachine<MyBot.State, MyBot.Action>(MyBot.State.Auth);
            Console.WriteLine($"Start_State: {bot.State} \n"); // Точка входа - Auth

            bot.Configure(MyBot.State.Auth)
                .OnEntry(s => Console.WriteLine($"Entry to {s.Destination} in {s.Source}"))
                .OnExit(s => Console.WriteLine($"Exit in {s.Source}, Destination {s.Destination}"))
                .PermitReentry(MyBot.Action.GetAuth)
                //  .PermitIf(MyBot.Action.GetMainMenu, MyBot.State.MainMenu, guard => userIsAuth==false, "Auth failed")
                .Permit(MyBot.Action.GetMainMenu, MyBot.State.MainMenu);


            bot.Configure(MyBot.State.MainMenu)
                .OnEntry(s => Console.WriteLine($"Entry to {s.Destination} in {s.Source}"))
                .OnExit(s => Console.WriteLine($"Exit in {s.Source}, Destination {s.Destination}"))
                .Permit(MyBot.Action.GetVotes, MyBot.State.Votes)
                .Permit(MyBot.Action.GetHelp, MyBot.State.Help);

            
            bot.Configure(MyBot.State.Votes)
                .OnEntryAsync
                (s => 
                    { Console.WriteLine($"Entry ASYNC to {s.Destination} in {s.Source}");
                        return Votes.RunVotes(bot, true); })
                .OnExit(s => Console.WriteLine($"Exit in {s.Source}, Destination {s.Destination}"))
                .Permit(MyBot.Action.GetMainMenu, MyBot.State.MainMenu)
                .Permit(MyBot.Action.GetVotesDB, MyBot.State.OnVotesDB);
            
            bot.Configure(MyBot.State.OnVotesDB)
                .OnEntry(s => Console.WriteLine($"Entry to {s.Destination} in {s.Source}"))
                .OnExit(s   => Console.WriteLine($"Exit in {s.Source}, Destination {s.Destination}"))
                .Permit(MyBot.Action.GetVotes, MyBot.State.Votes);


            bot.Configure(MyBot.State.Help)
                .OnEntry(s => Console.WriteLine($"Entry to {s.Destination} in {s.Source}"))
                .OnExit(s => Console.WriteLine($"Exit in {s.Source}, Destination {s.Destination}"))
                .Permit(MyBot.Action.GetMainMenu, MyBot.State.MainMenu);
          


            // string graph = UmlDotGraph.Format(bot.GetInfo());
            // Console.WriteLine("GPAPH||||||||||  \n" + graph + "\n||||||||||");
            
             bot.Fire(MyBot.Action.GetMainMenu);
             Console.WriteLine($"CurrentState: {bot.State}\n");
            
             bot.FireAsync(MyBot.Action.GetVotes);
             Console.WriteLine($"CurrentState: {bot.State}\n");

        }
    }
}

