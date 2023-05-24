using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BottApp.Host.Controllers.Client;

[ApiController]
[ApiExplorerSettings(GroupName = "v1")]
[Produces("application/json")]
[Route("bot/5436952910:AAFPumwqjaZsT-IKybbvcxIUxVpdVrzFth4/[controller]/[action]")]
public class AbstractClientController<T> : AbstractBaseController<T>
{
    public AbstractClientController(ILogger<T> logger) : base(logger)
    {
    }
}