
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoteApp.Client.User;
using VoteApp.Database;

namespace VoteApp.Host.Controllers.Admin;

public class AdminTestController : AbstractClientController
{
    
    public AdminTestController(IDatabaseContainer databaseContainer) : base(databaseContainer) { }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> UserById(int id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }
      
        var response = await DatabaseContainer.UserWeb.GetOneById(id);
        return Ok(response);
    }
   
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateUser(RequestRegisterUser requestRegisterUser)
    {
        var user = await DatabaseContainer.UserWeb.CreateUser(requestRegisterUser.Login, requestRegisterUser.FirstName, requestRegisterUser.LastName, requestRegisterUser.Phone, requestRegisterUser.Password);
        return Ok(user);
    }


}