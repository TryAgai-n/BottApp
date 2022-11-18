using Stateless.Graph;
using Stateless;

namespace StateMachine.Bot;

public static class Votes
{
    public static async Task RunVotes (StateMachine<MState,MAction> bot, bool condition)
    { 
        Console.WriteLine($"TASK RUN: {bot.State} \n");
        if (condition)
            await bot.FireAsync(MAction.GetMainMenu);
        else
            await bot.FireAsync(MAction.GetAuth); //Excpetion потому что мы не можем перейти из Votes в GetAuth
 
    }
}