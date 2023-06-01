using BottApp.Database.Document;
using Microsoft.AspNetCore.Mvc;

namespace BottApp.Host.Service;

public interface IDocumentService
{
    Task<IActionResult> CreateZipArchive(List<DocumentModel> documents, DocumentQuality documentQuality);

    Task UploadDocument(int userId, IFormFile photo, DocumentStatus documentStatus);

    Task<IActionResult> GetDocumentFile(DocumentModel document, DocumentQuality documentQuality);
}