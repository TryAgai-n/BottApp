using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BottApp.Database.Document.Like;
using BottApp.Database.Document.Statistic;
using BottApp.Database.User;

namespace BottApp.Database.Document;

public class DocumentModel : AbstractModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int UserId { get; set; }

    [ForeignKey("UserId")]
    public UserModel UserModel { get; set; }

    public string? DocumentType { get; set; }

    public string? DocumentExtension { get; set; }

    public string? Path { get; set; }
    
    [DataType(DataType.Date)]
    public DateTime CreatedAt { get; set; }
    
    [Required]
    public DocumentInPath DocumentInPath { get; set; }

    public DocumentStatisticModel DocumentStatisticModel { get; set; }
    
    public List<LikedDocumentModel> Likes { get; set; }

    
    public static DocumentModel CreateModel(
        int userId,
        string? documentType,
        string? documentExtension,
        DateTime createdAt,
        string? path,
        DocumentInPath documentInPath
    )
    {
        return new DocumentModel
        {
            UserId = userId,
            DocumentExtension = documentExtension,
            DocumentType = documentType,
            CreatedAt = createdAt,
            Path = path,
            DocumentInPath = documentInPath,
            DocumentStatisticModel = DocumentStatisticModel.CreateEmpty(),
            Likes = new List<LikedDocumentModel>()
        };
    }
}