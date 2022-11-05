using System;
using System.Threading.Tasks;
using BottApp.Database.Message;
using Microsoft.Extensions.Logging;

namespace BottApp.Database.Document;

public class DocumentRepository : AbstractRepository<DocumentModel>, IDocumentRepository
{
    public DocumentRepository(PostgreSqlContext context, ILoggerFactory loggerFactory) : base(context, loggerFactory)
    {
    }
    
    public async Task<DocumentModel> CreateModel(int userId, string? documentType, string? documentExtension, DateTime createdAt)
    {
        var model = DocumentModel.CreateModel(userId, documentType, documentExtension, createdAt);

        var result = await CreateModelAsync(model);
        if (result == null)
        {
            throw new Exception("Document model is not created");
        }

        return result;
    }
}