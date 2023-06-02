
using ICSharpCode.SharpZipLib.Zip;
using ImageMagick;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using VoteApp.Database;
using VoteApp.Database.Document;
using CompressionMethod = ImageMagick.CompressionMethod;

namespace VoteApp.Host.Service;

public class DocumentService : IDocumentService
{
    private readonly IDatabaseContainer _databaseContainer;

    public DocumentService(IDatabaseContainer databaseContainer)
    {
        _databaseContainer = databaseContainer;
    }

    public async Task<IActionResult> CreateZipArchive(List<DocumentModel> documents, DocumentQuality documentQuality)
    {
        if (documents.Count == 0)
        {
            return new NotFoundResult();
        }
        
        var tempFolderPath = Path.Combine(Path.GetTempPath(), "TempFiles");
        Directory.CreateDirectory(tempFolderPath);

        var zipFilePath = Path.Combine(tempFolderPath, "files.zip");

        await using (var zipStream = File.Create(zipFilePath))
        {
            await using (var zipOutputStream = new ZipOutputStream(zipStream))
            {
                foreach (var document in documents)
                {

                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), document.Path);

                    if (!File.Exists(filePath))
                    {
                        continue;
                    }

                    await using var fileStream = File.OpenRead(filePath);

                    var fileName = Path.GetFileName(filePath);

                    if (string.IsNullOrEmpty(fileName))
                    {
                        continue;
                    }

                    var entry = new ZipEntry(fileName)
                    {
                        Size = fileStream.Length
                    };

                    await zipOutputStream.PutNextEntryAsync(entry);

                    await fileStream.CopyToAsync(zipOutputStream);
                }

                zipOutputStream.Finish();
                zipOutputStream.Close();
            }
        }

        var fileBytes = await File.ReadAllBytesAsync(zipFilePath);
        File.Delete(zipFilePath);

        return new FileContentResult(fileBytes, "application/zip")
        {
            FileDownloadName = "files.zip"
        };
    }


    public async Task UploadDocument(int userId, IFormFile photo, DocumentStatus documentStatus)
    {
        var projectDirectory = Directory.GetCurrentDirectory();
        var dataDirectory = Path.Combine(projectDirectory, "Data");
        var currentDate = DateTime.Now.ToString("dd.MM.yyyy");

        var documentExtension = Path.GetExtension(photo.FileName);

        var filePathFullQuality = Path.Combine(dataDirectory, $"FULL.{currentDate}.{Guid.NewGuid()}{documentExtension}");

        var directoryFullPath = Path.GetDirectoryName(filePathFullQuality);
        
        if (!Directory.Exists(directoryFullPath))
        {
            Directory.CreateDirectory(directoryFullPath);
        }

        await using (var stream = new FileStream(filePathFullQuality, FileMode.Create))
        {
            await photo.CopyToAsync(stream);
        }
        var relativePath = Path.GetRelativePath(projectDirectory, filePathFullQuality);
        var model = DocumentModel.CreateModel(userId, documentExtension, relativePath, documentStatus);

        await _databaseContainer.Document.CreateModel(model);
    }
    
    public async Task<IActionResult> GetDocumentFile(DocumentModel document, DocumentQuality documentQuality)
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), document.Path);

        if (!File.Exists(filePath))
        {
            return new NotFoundResult();
        }

        var cacheDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Data", ".cache");
        
        if (!Directory.Exists(cacheDirectory))
        {
            Directory.CreateDirectory(cacheDirectory);
        }

        var fileName = $"{GetImageQualityPrefix(documentQuality)}.ID{document.Id}.{document.DocumentExtension}";
        var compressPath = Path.Combine(cacheDirectory, fileName);
        
        if (!File.Exists(compressPath))
        {
            await CompressFile(filePath, compressPath, documentQuality);
        }

        var fileExtension = Path.GetExtension(compressPath);
        var mimeType = GetMimeType(fileExtension);
        var fileStream = File.OpenRead(compressPath);

        return new FileStreamResult(fileStream, mimeType)
        {
            FileDownloadName = fileName
        };
    }

    private async Task CompressFile(string filePath, string compressPath, DocumentQuality documentQuality)
    {
        using var image = new MagickImage(filePath);

        image.Format = MagickFormat.Jpeg;
        
        switch (documentQuality)
        {
            case DocumentQuality.High:
                image.Quality = 90;
                image.Interlace = Interlace.Line;
                image.FilterType = FilterType.Lanczos;
                break;
            case DocumentQuality.Medium:
                image.Quality = 70;
                image.Interlace = Interlace.Line;
                image.FilterType = FilterType.Triangle;
                break;
            case DocumentQuality.Low:
                image.Quality = 50;
                image.Interlace = Interlace.Plane;
                image.FilterType = FilterType.Catrom;
                break;
            case DocumentQuality.UltraLow:
                image.Quality = 100;
                image.Interlace = Interlace.Line;
                image.FilterType = FilterType.Lanczos;
                image.Resize(new MagickGeometry(256, 256));
                break;
            default:
                image.Quality = 70;
                image.Interlace = Interlace.Line;
                image.FilterType = FilterType.Triangle;
                break;
        }

        image.Strip();
        await image.WriteAsync(compressPath);

    }

    private int GetImageQuality(DocumentQuality documentQuality)
    {
        return documentQuality switch
        {
            DocumentQuality.High => 50,
            DocumentQuality.Medium => 30,
            DocumentQuality.Low => 15,
            DocumentQuality.UltraLow => 3,
            _ => 30
        };
    }

    private string GetImageQualityPrefix(DocumentQuality documentQuality)
    {
        return documentQuality switch
        {
            DocumentQuality.High => "High",
            DocumentQuality.Medium => "Medium",
            DocumentQuality.Low => "Low",
            DocumentQuality.UltraLow => "UltraLow",
            _ => "Unknown"
        };
    }

    private string GetMimeType(string fileExtension)
    {
        switch (fileExtension.ToLower())
        {
            case ".jpeg":
            case ".jpg":
                return "image/jpeg";

            case ".png":
                return "image/png";

            case ".pdf":
                return "application/pdf";

            default:
                return "application/octet-stream";
        }
    }
    

}