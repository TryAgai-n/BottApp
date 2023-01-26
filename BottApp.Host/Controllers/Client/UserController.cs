using BottApp.Database;
using BottApp.Host.Filters;
using BottApp.Host.Handlers;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BottApp.Host.Controllers.Client;

public class UserController : ControllerBase
{
    

    [HttpPost]
    [ValidateTelegramBot]
    public async Task<IActionResult> Post(
        [FromBody] Update update,
        [FromServices] UpdateHandler handleUpdateService,
        CancellationToken cancellationToken
    )
    {
        await handleUpdateService.HandleUpdateAsync(update, cancellationToken);
        return Ok();
    }
}