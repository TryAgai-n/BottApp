using BottApp.Host.Handlers;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Examples.WebHook.Filters;
using Telegram.Bot.Examples.WebHook.Services;
using Telegram.Bot.Types;

namespace Telegram.Bot.Examples.WebHook.Controllers;

public class BotController : ControllerBase
{
    [HttpPost]
    [ValidateTelegramBot]
    public async Task<IActionResult> Post(
        [FromBody] Update update,
        [FromServices] UpdateHandler handleUpdateService,
        CancellationToken cancellationToken)
    {
        await handleUpdateService.HandleUpdateAsync(update, cancellationToken);
        return Ok();
    }
}
