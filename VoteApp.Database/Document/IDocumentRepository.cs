using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace VoteApp.Database.Document;

public interface IDocumentRepository
{
    Task<DocumentModel> CreateModel(DocumentModel model);

    Task<List<DocumentModel>> ListDocumentsByStatus(DocumentStatus documentStatus, int skip, int take);

    Task<DocumentModel> GetDocumentById(int documentId);

    // Task<List<DocumentModel>> ListMostViewedDocuments(int skip = 0, int take = 10);
}