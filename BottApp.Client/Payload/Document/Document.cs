using System.ComponentModel.DataAnnotations;

namespace BottApp.Client.Payload.Document;

public class Document
{
    [Required]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public string? DocumentType { get; set; }

    [Required]
    public string? DocumentExtension { get; set; }

    [Required]
    public string? Path { get; set; }

    [Required]
    public int CreatedAt { get; set; }

    [Required]
    public Statistic Statistic { get; set; }
}