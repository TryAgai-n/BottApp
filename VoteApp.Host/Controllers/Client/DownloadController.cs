using VoteApp.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoteApp.Database.Document;
using VoteApp.Host.Service;

namespace VoteApp.Host.Controllers.Client;

public class DownloadController : AbstractClientController
{
    private readonly IDocumentService _documentService;
    public DownloadController(IDatabaseContainer databaseContainer, IDocumentService documentService) : base(databaseContainer)
    {
        _documentService = documentService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetDocumentsList(int skip, int take)
    {
        var documents = await DatabaseContainer.Document.ListDocumentsByStatus(DocumentStatus.Default, skip, take);
        
        if (documents.Count is 0)
        {
            return NoContent();
        }
        
        return Ok(documents);
    }
    
    
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> DownloadFiles(int skip, int take, DocumentQuality quality)
    {
        var documents = await DatabaseContainer.Document.ListDocumentsByStatus(DocumentStatus.Default, skip, take);

        if (documents.Count is 0)
        {
            return NoContent();
        }
        
        return await _documentService.CreateZipArchive(documents, quality);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> DownloadFile(int documentId, DocumentQuality quality)
    {
        var document = await DatabaseContainer.Document.GetDocumentById(documentId);
        return await _documentService.GetDocumentFile(document, quality);
    }
}