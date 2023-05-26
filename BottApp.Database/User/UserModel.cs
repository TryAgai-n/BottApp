using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BottApp.Database.Document;

namespace BottApp.Database.User;

public class UserModel : AbstractModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public string FirstName { get; set; }
    
    public string LastName { get; set; }
    
    public string Phone { get; set; }
    
    public string Password { get; set; }

    public List<DocumentModel> Documents;



    public static UserModel Create(string firstName, string lastName, string phone, string password)
    {
        return new UserModel
        {
            FirstName = firstName,
            LastName = lastName,
            Phone = phone,
            Password = password,
        };
    }
}