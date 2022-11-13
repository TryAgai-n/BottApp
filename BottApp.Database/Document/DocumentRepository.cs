using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    

    [Obsolete("Obsolete")]
    public Task IncrementViewById(int documentId, int viewCountIncrement = 1)
    {
        var commandText =
            $"UPDATE \"DocumentStatisticModel\" SET \"ViewCount\" = \"ViewCount\" + {{1}} WHERE \"DocumentId\" = {{0}}";
        return Context.Database.ExecuteSqlCommandAsync(commandText, documentId, viewCountIncrement);
    }


    public Task<List<DocumentModel>> ListMostViewedDocuments(int skip = 0, int take = 10)
    {
        return DbModel
            .OrderByDescending(x => x.DocumentStatisticModel.LikeCount)
            .Include(x => x.DocumentStatisticModel)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }
}