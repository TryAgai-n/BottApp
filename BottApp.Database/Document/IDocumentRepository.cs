using System;
using System.Threading.Tasks;

namespace BottApp.Database.Document;

public interface IDocumentRepository
{
    Task<DocumentModel> CreateModel(int userId, string? documentType, string? documentExtension, DateTime createdAt, string? path);

    Task IncrementViewById(int documentId, int viewCountIncrement = 1);
}