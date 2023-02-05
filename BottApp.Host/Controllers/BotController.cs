using BottApp.Host.Filters;
using BottApp.Host.Handlers;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace BottApp.Host.Controllers;

public class BotController : ControllerBase
{
    [HttpPost]
    [ValidateTelegramBot]
    public async Task<IActionResult> Post(
        [FromBody] Update update,
        [FromServices] UpdateHandler handleUpdateService,
        CancellationToken cancellationToken)
    {
        try
        {
            await handleUpdateService.HandleUpdateAsync(update, cancellationToken);
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n\n{e} \n\n");
            return Ok();
        }
        return Ok();
    }
}
