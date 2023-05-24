using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BottApp.Database.Document;

public class DocumentRepository : AbstractRepository<DocumentModel>, IDocumentRepository
{
    public DocumentRepository(PostgresContext context, ILoggerFactory loggerFactory) : base(context, loggerFactory)
    {
    }
    
    public async Task<DocumentModel> CreateModel(int userId, string? documentType, string? documentExtension, DateTime createdAt, string? path, DocumentInPath documentInPath)
    {
        var model = DocumentModel.CreateModel(userId, documentType, documentExtension, createdAt, path, documentInPath);

        var result = await CreateModelAsync(model);
        if (result == null)
        {
            throw new Exception("Document model is not created");
        }

        return result;
    }


    public async Task<DocumentModel> GetOneByDocumentId(int documentId)
    {
        var model = await DbModel.Where(x => x.Id == documentId).Include(x => x.DocumentStatisticModel).FirstAsync();
        if(model == null)
        {
            throw new Exception($"Document with id: {documentId} is not found");
        }
        return model;
    }


    public Task<List<DocumentModel>> ListDocumentsByPath(Pagination pagination, DocumentInPath documentInPath)
    {
        return DbModel
            .Where(x => x.DocumentInPath == documentInPath)
            .OrderByDescending(x => x.Id)
            .Include(x => x.DocumentStatisticModel)
            .Skip(pagination.GetSkip())
            .Take(pagination.Limit)
            .ToListAsync();
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