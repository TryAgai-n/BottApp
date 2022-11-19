using System.ComponentModel.DataAnnotations;
using BottApp.Client.Payload.Message;

namespace BottApp.Client.Payload.User;

public class User
{
    [Required]
    public int Id { get; set; }

    [Required]
    public long UId { get; set; }

    [Required]
    public string? FirstName { get; set; }

    [Required]
    public string? Phone { get; set; }

    [Required]
    public OnState OnState { get; set; }

    [Required]
    public List<Message.Message> Messages { get; set; }

    [Required]
    public List<Document.Document> Documents { get; set; }
}