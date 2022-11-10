using Stateless;
using Stateless.Graph;

namespace StateMachine.Bot;

 public static class Auth
{
    public static async Task RunAuth (StateMachine<MState,MAction> bot)
    {
        Console.WriteLine($"TASK RUN:  {bot.State} \n");
        await bot.FireAsync(MAction.GetMainMenu);
    }
}