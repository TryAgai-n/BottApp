using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BottApp.Database.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BottApp.Database.Document;

public class DocumentRepository : AbstractRepository<DocumentModel>, IDocumentRepository
{

    public DocumentRepository(PostgreSqlContext context, ILoggerFactory loggerFactory) : base(context, loggerFactory) { }


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
        var model = DocumentModel.CreateModel(userId, documentType, documentExtension, createdAt, path, caption, documentInPath, documentNomination);

        var result = await CreateModelAsync(model);

        if (result == null)
        {
            throw new Exception("Document model is not created");
        }

        return result;
    }


    public async Task<DocumentModel> CreateModel(
        int userId,
        InNomination nomination,
        DocumentInPath path,
        DateTime createAt
    )
    {
        var model = DocumentModel.CreateModel(userId, nomination, path, createAt);

        var result = await CreateModelAsync(model);

        if (result == null)
        {
            throw new Exception("Empty document model is not created");
        }

        return result;
    }


    public async Task<DocumentModel> GetOneByDocumentId(int documentId)
    {
        var model = await DbModel.Where(x => x.Id == documentId).Include(x => x.DocumentStatisticModel).FirstAsync();

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
        bool isOrderByView = false,
        bool isModerate = false
    )
    {
        return await PrepareDocumentNomination(documentNomination)
            .OrderBy(x => isOrderByView ? x.DocumentStatisticModel.ViewCount : x.DocumentStatisticModel.Id)
            .Include(x => x.DocumentStatisticModel)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }


    public Task<List<DocumentModel?>> GetListByNomination(InNomination? documentNomination, bool isModerate = true)
    {
        return PrepareDocumentNomination(documentNomination)
            .Where(x => isModerate ? x.DocumentStatisticModel.IsModerated : !x.DocumentStatisticModel.IsModerated)
            .Include(x => x.DocumentStatisticModel)
            .OrderBy(x => x.Id)
            .ToListAsync();
    }


    public async Task<int> GetCountByNomination(InNomination? documentNomination)
    {
        return await PrepareDocumentNomination(documentNomination).CountAsync();
    }


    public async Task<List<DocumentModel>> GetCountDocumentByPath(DocumentInPath documentInPath)
    {
        return await PrepareDocumentPath(documentInPath)
            .OrderBy(x => x.Id)
            .ToListAsync();
    }


    public async Task<bool> CheckSingleDocumentInNominationByUser(UserModel user, InNomination? documentNomination)
    {
        var result = await PrepareDocumentNomination(documentNomination)
            .FirstOrDefaultAsync(x => x.UserModel.Id == user.Id);


        return false;
    }


    public async Task<bool> SetModerate(int documentId, bool isModerate)
    {
        var model = await GetOneByDocumentId(documentId);

        if (model is null)
        {
            return false;
        }

        model.DocumentStatisticModel.IsModerated = isModerate;

        await UpdateModelAsync(model);
        return true;
    }


    public async Task<List<DocumentModel>> ListMostViewedDocuments(int skip = 0, int take = 10)
    {
        return DbModel.OrderByDescending(x => x.DocumentStatisticModel.ViewCount)
            .Include(x => x.DocumentStatisticModel)
            .Skip(skip)
            .Take(take)
            .ToList();
    }


    public async Task<List<DocumentModel>> List_Most_Document_In_Vote_By_Views(int take)
    {
        return DbModel
            .Where(x => x.Path != null)
            .Include(x => x.DocumentStatisticModel)
            .OrderByDescending(x => x.DocumentStatisticModel.ViewCount)
            .ToList()
            .OrderBy(x => x.DocumentNomination)
            .GroupBy(x => x.DocumentNomination)
            .SelectMany(x => x.Take(take))
            .ToList();
    }


    public async Task<List<DocumentModel>> List_Most_Document_In_Vote_By_Likes(int take)
    {
        return DbModel.Where(x => x.Path != null)
            .Include(x => x.DocumentStatisticModel)
            .OrderByDescending(x => x.DocumentStatisticModel.LikeCount)
            .ToList()
            .OrderBy(x => x.DocumentNomination)
            .GroupBy(x => x.DocumentNomination)
            .SelectMany(x => x.Take(take))
            .ToList();
    }


    public async Task IncrementViewByDocument(DocumentModel model)
    {
        model.DocumentStatisticModel.ViewCount++;
        await UpdateModelAsync(model);
    }


    public async Task IncrementLikeByDocument(DocumentModel model)
    {
        model.DocumentStatisticModel.LikeCount++;
        await UpdateModelAsync(model);
    }

    public async Task<bool> UpdateDocument(DocumentModel? model)
    {
        if (model is null) return false;
        await UpdateModelAsync(model);
        return true;
    }
}