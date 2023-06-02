using System.ComponentModel.DataAnnotations;

namespace VoteApp.Client.User;

public class RequestLoginUser
{
    [Required]
    public string Login { get; set; }
    [Required]
    public string Password { get; set; }

}