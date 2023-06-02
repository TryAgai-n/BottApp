
using Microsoft.AspNetCore.Mvc;
using VoteApp.Database.Document;

namespace VoteApp.Host.Service;

public interface IDocumentService
{
    Task<IActionResult> CreateZipArchive(List<DocumentModel> documents, DocumentQuality documentQuality);

    Task UploadDocument(int userId, IFormFile photo, DocumentStatus documentStatus);

    Task<IActionResult> GetDocumentFile(DocumentModel document, DocumentQuality documentQuality);
}