using Stateless;

namespace BottApp.Host.StateMachine;

public static class Votes
{
    public static async Task RunVotes (StateMachine<StateStatus,ActionStatus> bot, bool condition)
    { 
        Console.WriteLine($"TASK RUN: {bot.State} \n");
        if (condition)
            await bot.FireAsync(ActionStatus.GetMainMenu);
        else
            await bot.FireAsync(ActionStatus.GetAuth); //Excpetion потому что мы не можем перейти из Votes в GetAuth
 
    }
}