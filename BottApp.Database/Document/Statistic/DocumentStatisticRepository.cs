
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BottApp.Database.Document.Statistic;

public class DocumentStatisticRepository : AbstractRepository<DocumentStatisticModel>, IDocumentStatisticRepository
{
    public DocumentStatisticRepository(PostgresContext context, ILoggerFactory loggerFactory) : base(context, loggerFactory)
    {
    }
    
    
    
    public async Task<List<DocumentStatisticModel>> ListTopDocuments(int skip, int take = 10)
    {
        return await DbModel
            .Include(x => x.DocumentModel)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<DocumentStatisticModel> CreateModel(int viewCount, int likeCount)
    {
        var model = DocumentStatisticModel.CreateModel(viewCount, likeCount);

        var result = await CreateModelAsync(model);
        
        if (result == null)
        {
            
        }

        return result;
    }
    
    
    
    //TODO:
    // [Obsolete("Obsolete")]
    // public Task IncrementViewById(int documentId, int viewCountIncrement = 1)
    // {
        // var commandText =
            // $"UPDATE \"DocumentStatisticModel\" SET \"ViewCount\" = \"ViewCount\" + {viewCountIncrement} WHERE \"DocumentId\" = {documentId}";
        // return Context.Database.ExecuteSqlCommandAsync(commandText, documentId, viewCountIncrement);
    // }
}