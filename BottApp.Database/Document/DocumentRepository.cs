using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageMagick;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BottApp.Database.Document;

public class DocumentRepository : AbstractRepository<DocumentModel>, IDocumentRepository
{
    public DocumentRepository(PostgresContext context, ILoggerFactory loggerFactory) : base(context, loggerFactory)
    { }


    public async Task<DocumentModel> CreateModel(DocumentModel model)
    {
        var result = await CreateModelAsync(model);

        if (result == null)
        {
            throw new Exception("Document model is not created");
        }

        return result;
    }


    public Task<List<DocumentModel>> ListDocumentsByStatus(DocumentStatus documentStatus, int skip, int take)
    {
        return DbModel.Where(x => x.DocumentStatus == documentStatus)
            .OrderBy(x => x.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }


    public async Task<DocumentModel> GetDocumentById(int documentId)
    {
        var model = await DbModel.Where(x => x.Id == documentId).FirstAsync();

        if (model is null)
        {
            throw new Exception($"Document with Id: {documentId} is not found");
        }

        return model;
    }
    
}