using Stateless.Graph;
using Stateless;

namespace StateMachine.Bot;

public static class Votes
{
    public static async Task RunVotes (StateMachine<MyBot.State,MyBot.Action> bot, bool condition)
    { 
        Console.WriteLine($"TASK RUN: {bot.State} \n");
        if (condition)
            await bot.FireAsync(MyBot.Action.GetMainMenu);
        else
            await bot.FireAsync(MyBot.Action.GetAuth); //Excpetion потому что мы не можем перейти из Votes в GetAuth
 
    }
}