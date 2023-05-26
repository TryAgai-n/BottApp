using BottApp.Database;
using BottApp.Database.Document;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.AspNetCore.Mvc;

namespace BottApp.Host.Service;

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

        await using (var zipStream = System.IO.File.Create(zipFilePath))
        {
            await using (var zipOutputStream = new ZipOutputStream(zipStream))
            {
                foreach (var document in documents)
                {
                    var path = documentQuality switch
                    {

                        DocumentQuality.Full => document.PathFullQuality,
                        DocumentQuality.Half => document.PathHalfQuality,
                    };
                    
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), path);

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

    public async Task<IActionResult> GetDocumentFile(DocumentModel document, DocumentQuality documentQuality)
    {
        var path = documentQuality switch
        {

            DocumentQuality.Full => document.PathFullQuality,
            DocumentQuality.Half => document.PathHalfQuality,
        };


        var filePath = Path.Combine(Directory.GetCurrentDirectory(), path);

        if (!File.Exists(filePath))
        {
            return new NotFoundResult();
        }

        var fileExtension = Path.GetExtension(filePath);
        var mimeType = GetMimeType(fileExtension);

        var fileStream = File.OpenRead(filePath);
        var fileName = Path.GetFileName(filePath);

        return new FileStreamResult(fileStream, mimeType)
        {
            FileDownloadName = fileName
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