using VoteApp.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VoteApp.Host.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]/[action]")]
public abstract class AbstractClientController: ControllerBase
{
    protected readonly IDatabaseContainer DatabaseContainer;

    protected AbstractClientController(IDatabaseContainer databaseContainer)
    {
        DatabaseContainer = databaseContainer;
    }
}