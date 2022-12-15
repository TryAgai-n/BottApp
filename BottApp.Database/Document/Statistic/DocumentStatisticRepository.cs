using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;

namespace BottApp.Database.Document.Statistic;

public class DocumentStatisticRepository : AbstractRepository<DocumentStatisticModel>, IDocumentStatisticRepository
{
    public DocumentStatisticRepository(PostgreSqlContext context, ILoggerFactory loggerFactory) : base(context, loggerFactory)
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

    public async Task<DocumentStatisticModel> CreateModel(int documentId, int viewCount, int likeCount)
    {
        var model = DocumentStatisticModel.CreateModel(documentId, viewCount, likeCount);

        var result = await CreateModelAsync(model);
        
        if (result == null)
        {
            throw new Exception($"DocumentStatisticModel ");
        }

        return result;
    }
}