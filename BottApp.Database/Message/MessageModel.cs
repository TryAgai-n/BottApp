using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BottApp.Database.User;

namespace BottApp.Database.Message;

public class MessageModel : AbstractModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public int UserId { get; set; }
    
    [ForeignKey("UserId")]
    public UserModel UserModel { get; set; }
    
    
    public string? Description { get; set; }


    public static MessageModel CreateModel(int userId, string? description)
    {
        return new MessageModel
        {
            UserId = userId,
            Description = description
        };
    }
}