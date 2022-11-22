using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BottApp.Database.User;

namespace BottApp.Database.Document;

public interface IDocumentRepository
{
    Task<DocumentModel> CreateModel(
        int userId,
        string? documentType,
        string? documentExtension,
        DateTime createdAt,
        string? path,
        DocumentInPath documentInPath
    );


    Task<DocumentModel> GetOneByDocumentId(int documentId);
    
    Task<DocumentModel> GetFirstDocumentByPath(DocumentInPath documentInPath);

    Task<List<DocumentModel>> ListDocumentsByPath(Pagination pagination, DocumentInPath documentInPath);
    
    Task<List<DocumentModel>> ListMostViewedDocuments(int skip = 0, int take = 10);
}