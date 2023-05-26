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


    public async Task<DocumentModel> CreateModel(int userId, IFormFile photo, DocumentStatus documentStatus)
    {
        var projectDirectory = Directory.GetCurrentDirectory();
        var dataDirectory = Path.Combine(projectDirectory, "Data");
        var currentDate = DateTime.Now.ToString("dd.MM.yyyy");
        
        var documentExtension = Path.GetExtension(photo.FileName);

        var filePathFullQuality = Path.Combine(dataDirectory, $"{userId}\\Full\\FULL.{currentDate}.{Guid.NewGuid()}{documentExtension}");
        var filePathHalfQuality = Path.Combine(dataDirectory, $"{userId}\\Half\\HALF.{currentDate}.{Guid.NewGuid()}{documentExtension}");
        
        var directoryFullPath = Path.GetDirectoryName(filePathFullQuality);
        
        if (!Directory.Exists(directoryFullPath))
        {
            Directory.CreateDirectory(directoryFullPath);
        }
        
        var directoryHalfPath = Path.GetDirectoryName(filePathHalfQuality);
        
        if (!Directory.Exists(filePathHalfQuality))
        {
            Directory.CreateDirectory(directoryHalfPath);
        }

        await using (var stream = new FileStream(filePathFullQuality, FileMode.Create))
        {
            await photo.CopyToAsync(stream);
        }
        
        await CompressFile(filePathHalfQuality, filePathFullQuality);

        var relativeFullPath = Path.GetRelativePath(projectDirectory, filePathFullQuality);
        var relativeHalfPath = Path.GetRelativePath(projectDirectory, filePathHalfQuality);

        var model = DocumentModel.CreateModel(userId, documentExtension, relativeFullPath, relativeHalfPath, documentStatus);

        var result = await CreateModelAsync(model);

        if (result == null)
        {
            throw new Exception("Document model is not created");
        }

        return result;
    }
    
    private async Task CompressFile(string compressedFilePath, string fullQualityFilePath)
    {
        
        using var image = new MagickImage(fullQualityFilePath);
        image.Format = image.Format;
        image.Quality = 30;
        await image.WriteAsync(compressedFilePath);

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