using BottApp.Database.Document;
using Microsoft.AspNetCore.Mvc;

namespace BottApp.Host.Service;

public interface IDocumentService
{
    Task<IActionResult> CreateZipArchive(List<DocumentModel> documents, DocumentQuality documentQuality);

    Task<IActionResult> GetDocumentFile(DocumentModel document, DocumentQuality documentQuality);
}