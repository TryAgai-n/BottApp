using StateMachine.Bot;

namespace StateMachine
{
    public class Program
    { 
        public static void Main(string[] args)
        {
            bool userIsAuth = false;
            
            var bot = new Stateless.StateMachine<MyBot.State, MyBot.Action>(MyBot.State.Auth);
            Console.WriteLine($"Start_State: {bot.State} \n");

            bot.Configure(MyBot.State.Auth)
                .PermitIf(MyBot.Action.GetMainMenu, MyBot.State.MainMenu);


            bot.Configure(MyBot.State.MainMenu)
                .Permit(MyBot.Action.GetVotes, MyBot.State.Votes)
                .Permit(MyBot.Action.GetHelp, MyBot.State.Help)
                .OnEntry( s=>  Console.WriteLine($"Entry to {s.Destination} in {s.Source}"))
                .OnExit(s   => Console.WriteLine($"Exit in {s.Source}, Destination {s.Destination}"));
            
            bot.Configure(MyBot.State.OnVotesDB)
                .Permit(MyBot.Action.GetVotes, MyBot.State.Votes)
                .OnEntry(s => Console.WriteLine($"Entry to {s.Destination} in {s.Source}"))
                .OnExit(s   => Console.WriteLine($"Exit in {s.Source}, Destination {s.Destination}"));
            
            bot.Configure(MyBot.State.Votes)
                .Permit(MyBot.Action.GetMainMenu, MyBot.State.MainMenu)
                .Permit(MyBot.Action.GetVotesDB, MyBot.State.OnVotesDB)
                .OnEntry(s => Console.WriteLine($"Entry to {s.Destination} in {s.Source}"))
                .OnExit(s   => Console.WriteLine($"Exit in {s.Source}, Destination {s.Destination}"));

            bot.Configure(MyBot.State.Help)
                .Permit(MyBot.Action.GetMainMenu, MyBot.State.MainMenu)
                .OnEntry(s => Console.WriteLine($"Entry to {s.Destination} in {s.Source}"))
                .OnExit(s   => Console.WriteLine($"Exit in {s.Source}, Destination {s.Destination}"));

        
            
            bot.Fire(MyBot.Action.GetMainMenu);
            Console.WriteLine($"CurrentState: {bot.State}");
            
            bot.Fire(MyBot.Action.GetVotes);
            Console.WriteLine($"CurrentState: {bot.State}");
            
            bot.Fire(MyBot.Action.GetVotesDB);
            Console.WriteLine($"CurrentState: {bot.State}");
            
            bot.Fire(MyBot.Action.GetMainMenu);
            Console.WriteLine($"CurrentState: {bot.State}");
            
            bot.Fire(MyBot.Action.GetVotesDB);//Exception, потому что мы можем вызвать загрузку в базу находясь только в State Votes
            Console.WriteLine($"CurrentState: {bot.State}");
            
        }
    }
}

