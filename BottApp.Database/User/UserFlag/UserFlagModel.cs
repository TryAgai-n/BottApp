using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BottApp.Database.User.UserFlag;

public class UserFlagModel : AbstractModel
{ 
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [ForeignKey("UserId")]
    public UserModel UserModel { get; set; }
    
    public bool IsSendLastName { get; set; }

    public bool IsSendFirstName { get; set; }
  
    
    public bool IsSendCaption { get; set; }

    public bool IsSendDocument { get; set; }

    public bool IsSendNomination { get; set; }

    public bool IsSendPhone { get; set; }

  
    
    
    public static UserFlagModel CreateModel()
    {
        return new UserFlagModel
        { };
    }
}