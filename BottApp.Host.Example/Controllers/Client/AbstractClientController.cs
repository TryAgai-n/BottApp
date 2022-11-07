using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BottApp.Host.Example.Controllers.Client;

[ApiController]
[ApiExplorerSettings(GroupName = "v1")]
[Produces("application/json")]
[Route("api/[controller]/[action]")]
public class AbstractClientController<T> : AbstractBaseController<T>
{
    public AbstractClientController(ILogger<T> logger) : base(logger)
    {
    }
}