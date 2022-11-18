using System.ComponentModel.DataAnnotations;

namespace BottApp.Client.Payload.Message;

public class Message
{
    [Required]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public string? Description { get; set; }

    [Required]
    public string? Type { get; set; }

    [Required]
    public int CreatedAt { get; set; }
}