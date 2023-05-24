using BottApp.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BottApp.Host.Controllers.Client;


public class UserController : AbstractClientController
{
   public UserController(IDatabaseContainer databaseContainer) : base(databaseContainer)
   { }
   
   [AllowAnonymous]
   [HttpGet]
   public async Task<IActionResult> UserById(int id)
   {
      var user = await _DatabaseContainer.User.GetOneById(id);
      return Ok(user);
   }

  
}