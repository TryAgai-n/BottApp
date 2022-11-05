using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BottApp.Database.User;
using BottApp.Utils;

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
    
    
    [Required]
    [DataType(DataType.Date)]
    public DateTime CreatedAt { get; set; }
    
    public static DocumentModel CreateModel(int userId, string? documentType, string? documentExtension, DateTime createdAt)
    {
        return new DocumentModel
        {
            UserId = userId,
            DocumentExtension = documentExtension,
            DocumentType = documentType,
            CreatedAt = createdAt
        };
    }
}