namespace BottApp.Host.StateMachine;
using Stateless;

public static class MachineStartup
{
    public static StateMachine<StateStatus, ActionStatus> FSM = new (StateStatus.MainMenu);
    

    public static void test()
    {
      FSM.Fire(ActionStatus.GetVotes);
    }
   
        public static void MachineConfig()
        {
            Console.WriteLine($"Start_State: {FSM.State} \n"); 

            FSM.Configure(StateStatus.MainMenu)
            .OnEntry(s => Console.WriteLine($"Entry to {s.Destination} in {s.Source}"))
            .OnExit(s => Console.WriteLine($"Exit in {s.Source}, Destination {s.Destination}"))
            .Permit(ActionStatus.GetVotes, StateStatus.Votes)
            .Permit(ActionStatus.GetHelp, StateStatus.Help)
            .PermitReentry(ActionStatus.GetMainMenu);
           
            
            FSM.Configure(StateStatus.Votes)
            .OnEntryAsync
            (s => 
                { 
                    Console.WriteLine($"Entry ASYNC to {s.Destination} in {s.Source}");
                    return Votes.RunVotes(FSM, true); 
                }
            )
            .OnExit(s => Console.WriteLine($"Exit in {s.Source}, Destination {s.Destination}"))
            .Permit(ActionStatus.GetMainMenu, StateStatus.MainMenu)
            .Permit(ActionStatus.GetVotesDB, StateStatus.OnVotesDB);


    }
}