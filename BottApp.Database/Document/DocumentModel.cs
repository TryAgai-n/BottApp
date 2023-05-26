using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BottApp.Database.Document.Like;
using BottApp.Database.Document.Statistic;
using BottApp.Database.User;
using BottApp.Database.WebUser;

namespace BottApp.Database.Document;

public class DocumentModel : AbstractModel
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public UserModel UserModel { get; set; }
    public string DocumentExtension { get; set; }
    public string PathFullQuality { get; set; }
    public string PathHalfQuality { get; set; }
    public DateTime CreatedAt { get; set; }
    public DocumentStatus DocumentStatus { get; set; }
    
    // public DocumentStatisticModel DocumentStatisticModel { get; set; }
    // public List<LikedDocumentModel> Likes { get; set; }

    public static DocumentModel CreateModel(
        int userId,
        string documentExtension,
        string pathFullQuality,
        string pathHalfQuality,
        DocumentStatus documentStatus
    )
    {
        return new DocumentModel
        {
            UserId = userId,
            DocumentExtension = documentExtension,
            CreatedAt = DateTime.UtcNow,
            PathFullQuality = pathFullQuality,
            PathHalfQuality = pathHalfQuality,
            DocumentStatus = documentStatus,
        };
    }
}