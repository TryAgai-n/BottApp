using BottApp.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BottApp.Host.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]/[action]")]
public abstract class AbstractClientController: ControllerBase
{
    public readonly IDatabaseContainer _DatabaseContainer;

    protected AbstractClientController(IDatabaseContainer databaseContainer)
    {
        _DatabaseContainer = databaseContainer;
    }
}