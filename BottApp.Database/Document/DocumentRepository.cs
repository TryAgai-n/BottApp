using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BottApp.Database.Message;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BottApp.Database.Document;

public class DocumentRepository : AbstractRepository<DocumentModel>, IDocumentRepository
{
    public DocumentRepository(PostgreSqlContext context, ILoggerFactory loggerFactory) : base(context, loggerFactory)
    {
    }
    
    public async Task<DocumentModel> CreateModel(int userId, string? documentType, string? documentExtension, DateTime createdAt, string? path)
    {
        var model = DocumentModel.CreateModel(userId, documentType, documentExtension, createdAt, path);

        var result = await CreateModelAsync(model);
        if (result == null)
        {
            throw new Exception("Document model is not created");
        }

        return result;
    }


    public async Task<List<DocumentModel>> ListTopDocuments(int skip, int take = 10)
    {
        return await DbModel
            .OrderByDescending(x => x.DocumentStatisticModel.LikeCount)
            .Include(x => x.DocumentStatisticModel)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }
}