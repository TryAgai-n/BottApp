using Microsoft.AspNetCore.Mvc;
using BottApp.Database;
using BottApp.Database.Document;
using Microsoft.AspNetCore.Authorization;


namespace BottApp.Host.Controllers.Client;


public class UploadController : AbstractClientController
{
    public UploadController(IDatabaseContainer databaseContainer) : base(databaseContainer) { }
    
    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile photo)
    {
        if (photo is null || photo.Length <= 0)
        {
            return BadRequest();
        }

        await DatabaseContainer.Document.CreateModel(1, photo, DocumentStatus.Default);

        return Ok();
    }
}
