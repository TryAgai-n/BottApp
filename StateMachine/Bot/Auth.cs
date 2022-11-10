using Stateless;
using Stateless.Graph;

namespace StateMachine.Bot;

 public static class Auth
{
    public static async Task RunAuth (StateMachine<MyBot.State,MyBot.Action> bot)
    {
        Console.WriteLine($"TASK RUN:  {bot.State} \n");
        await bot.FireAsync(MyBot.Action.GetMainMenu);
    }
}