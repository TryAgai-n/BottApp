using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BottApp.Database.User;
using Microsoft.AspNetCore.Http;

namespace BottApp.Database.Document;

public interface IDocumentRepository
{
    Task<DocumentModel> CreateModel(
        int userId,
        string? documentType,
        string? documentExtension,
        DateTime createdAt,
        string? path,
        DocumentStatus documentStatus
    );
    
    Task<DocumentModel> CreateModel(
        int userId,
        IFormFile photo,
        DocumentStatus documentStatus
    );


    // Task<DocumentModel> GetOneByDocumentId(int documentId);

    // Task<List<DocumentModel>> ListDocumentsByPath(Pagination pagination, DocumentInPath documentInPath);
    //
    // Task<List<DocumentModel>> ListMostViewedDocuments(int skip = 0, int take = 10);
}