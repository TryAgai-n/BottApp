using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BottApp.Host.Example.Controllers;

[ApiController]
public abstract class AbstractBaseController<T> : ControllerBase
{
    protected readonly ILogger<T> Logger;

    public AbstractBaseController(ILogger<T> logger)
    {
        Logger = logger;
    }
}