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
        string? caption,
        DocumentInPath documentInPath,
        DocumentNomination? documentNomination
    );


    Task<DocumentModel> GetOneByDocumentId(int documentId);
    
    Task<DocumentModel> GetFirstDocumentByPath(DocumentInPath documentInPath);
    Task<DocumentModel> GetFirstDocumentByNomination(DocumentNomination? documentNomination);
    Task<List<DocumentModel>> ListDocumentsByPath(DocumentInPath documentInPath);
    Task<List<DocumentModel>> ListDocumentsByNomination(Pagination pagination, DocumentNomination documentNomination);
    Task<List<DocumentModel>> GetListCountByNomination(DocumentNomination documentNomination);
    Task<List<DocumentModel>> ListMostViewedDocuments(int skip = 0, int take = 10);
}