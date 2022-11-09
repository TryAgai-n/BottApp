using BottApp.Host.Exp.Controllers;
using BottApp.Host.Exp.Services;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace BottApp.Host.Exp.Controllers;

public class WebhookController : AbstractBaseController<WebhookController>
{
    public WebhookController(ILogger<WebhookController> logger) : base(logger)
    {
    }
    
    
    [HttpPost]
    public async Task<IActionResult> Post([FromServices] HandleUpdateService handleUpdateService,
                                          [FromBody] Update update)
    {
        await handleUpdateService.EchoAsync(update);
        return Ok();
    }
}