using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BottApp.Data.User;

public class UserModel : AbstractModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public string FirstName { get; set; }

    [Required]
    public string Phone { get; set; }

    public bool IsSendContact { get; set; }


    public static UserModel Create(string firstName, string phone, bool isSendContact)
    {
        return new UserModel
        {
            FirstName = firstName,
            Phone = phone,
            IsSendContact = isSendContact
        };
    }
}