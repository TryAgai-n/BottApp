using System.Threading.Tasks;
using BottApp.Client.User;
using BottApp.Database;
using BottApp.Database.User;
using BottApp.Database.WebUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BottApp.Host.Controllers.Client;


public class UserController : AbstractClientController
{
   public UserController(IDatabaseContainer databaseContainer) : base(databaseContainer)
   { }
   
   [AllowAnonymous]
   [HttpPost]
   public async Task<IActionResult> UserById(RequestUser user)
   {
      if (!ModelState.IsValid)
      {
         return BadRequest();
      }
      
      var response = await DatabaseContainer.UserWeb.GetOneById(user.Id);
      return Ok(response);
   }
   
   [AllowAnonymous]
   [HttpPost]
   public async Task<IActionResult> CreateUser(UserModel user)
   {
      await DatabaseContainer.UserWeb.CreateUser(user.FirstName, user.LastName, user.Phone, user.Password);
      return Ok();
   }
}