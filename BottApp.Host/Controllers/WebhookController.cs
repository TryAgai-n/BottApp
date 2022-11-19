using BottApp.Host.Controllers;
using BottApp.Host.Services;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace BottApp.Host.Controllers;

[ApiController]
[Route("bot/5436952910:AAFPumwqjaZsT-IKybbvcxIUxVpdVrzFth4/[controller]/[action]")]
public class WebhookController : AbstractBaseController<WebhookController>
{
    public WebhookController(ILogger<WebhookController> logger) : base(logger)
    {
    }
    
    
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Update update)
    {
        return Ok();
    }
}