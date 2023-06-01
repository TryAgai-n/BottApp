using System.ComponentModel.DataAnnotations;

namespace BottApp.Client.User;

public class RequestLoginUser
{
    [Required]
    public string Login { get; set; }
    [Required]
    public string Password { get; set; }

}