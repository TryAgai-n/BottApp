using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace BottApp.Database.Document;

public interface IDocumentRepository
{
    Task<DocumentModel> CreateModel(
        int userId,
        IFormFile photo,
        DocumentStatus documentStatus
    );

    Task<List<DocumentModel>> ListDocumentsByStatus(DocumentStatus documentStatus, int skip, int take);

    Task<DocumentModel> GetDocumentById(int documentId);

    // Task<List<DocumentModel>> ListMostViewedDocuments(int skip = 0, int take = 10);
}