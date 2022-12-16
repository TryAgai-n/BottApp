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


    private IQueryable<DocumentModel> PrepareDocumentPath(DocumentInPath documentInPath)
    {
        return DbModel.Where(x => x.DocumentInPath == documentInPath);
    }


    private IQueryable<DocumentModel?> PrepareDocumentNomination(InNomination? documentNomination)
    {
        return DbModel.Where(x => x.DocumentNomination == documentNomination);
    }


    public async Task<DocumentModel> CreateModel(
        int userId,
        string? documentType,
        string? documentExtension,
        DateTime createdAt,
        string? path,
        string? caption,
        DocumentInPath documentInPath,
        InNomination? documentNomination
    )
    {
        var model = DocumentModel.CreateModel(
            userId, documentType, documentExtension, createdAt, path, caption, documentInPath, documentNomination
        );

        var result = await CreateModelAsync(model);
        if (result == null)
        {
            throw new Exception("Document model is not created");
        }

        return result;
    }


    public async Task<DocumentModel> GetOneByDocumentId(int documentId)
    {
        var model = await DbModel
            .Where(x => x.Id == documentId)
            .Include(x => x.DocumentStatisticModel)
            .FirstAsync();

        if (model == null)
        {
            throw new Exception($"Document with id: {documentId} is not found");
        }

        return model;
    }


    public async Task<DocumentModel> GetFirstDocumentByPath(DocumentInPath documentInPath)
    {
        var model = await PrepareDocumentPath(documentInPath).Include(x => x.DocumentStatisticModel).FirstAsync();

        if (model == null)
        {
            throw new Exception($"Document is not found");
        }

        return model;
    }


    public async Task<DocumentModel> GetFirstDocumentByNomination(InNomination? documentNomination)
    {
        var model = await PrepareDocumentNomination(documentNomination)
            .OrderBy(x => x.Id)
            .Include(x => x.DocumentStatisticModel)
            .FirstAsync();

        if (model.DocumentNomination == null)
        {
            throw new Exception($"Document by nomination is not found");
        }

        return model;
    }


    public async Task<List<DocumentModel>> ListDocumentsByPath(DocumentInPath documentInPath)
    {
        return await PrepareDocumentPath(documentInPath)
            .OrderBy(x => x.Id)
            .Include(x => x.DocumentStatisticModel)
            .ToListAsync();
    }


    public async Task<List<DocumentModel>> ListDocumentsByNomination(
        InNomination? documentNomination,
        int skip,
        int take,
        bool withOrderByView = false
    )
    {
        return await PrepareDocumentNomination(documentNomination)
            .OrderBy(x => withOrderByView ? x.DocumentStatisticModel.ViewCount : x.DocumentStatisticModel.Id)
            .ThenBy(x => x.DocumentStatisticModel.LikeCount)
            .Include(x => x.DocumentStatisticModel)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }
    
    public Task<List<DocumentModel?>> GetListByNomination(DocumentModel documentModel)
    {
        return PrepareDocumentNomination(documentModel.DocumentNomination)
            .OrderBy(x => x.Id)
            .ToListAsync();
        
        // return PrepareDocumentNomination(documentModel.DocumentNomination)
        //     .OrderBy(x => x.Id)
        //     .Select((item, index) => new { item, index })
        //     .FirstOrDefault(x=>x.item.Id == documentModel.Id).index;
    }


    public Task<int> GetCountByNomination(InNomination? documentNomination)
    {
        return PrepareDocumentNomination(documentNomination).CountAsync();
    }


    public Task<List<DocumentModel>> GetCountDocumentByPath(DocumentInPath documentInPath)
    {
        return PrepareDocumentPath(documentInPath).OrderBy(x => x.Id).ToListAsync();
    }


    public Task<List<DocumentModel>> ListMostViewedDocuments(int skip = 0, int take = 10)
    {
        return DbModel.OrderByDescending(x => x.DocumentStatisticModel.LikeCount).Include(x => x.DocumentStatisticModel)
            .Skip(skip).Take(take).ToListAsync();
    }
    
    
    public async Task IncrementViewByDocument(DocumentModel model)
    {
        model.DocumentStatisticModel.ViewCount++;
        await UpdateModelAsync(model);
    }
}