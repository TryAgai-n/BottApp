using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BottApp.Database.Document;
using BottApp.Database.Message;
using Microsoft.EntityFrameworkCore;

namespace BottApp.Database.User;

public class UserModel : AbstractModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Required]
    public long UId { get; set; }
    public string? FirstName { get; set; }
    public string? Phone { get; set; }
    public string? UserState { get; set; }
  
    
    public List<MessageModel> Messages { get; set; }
    
    public List<DocumentModel> Documents { get; set; }


    public static UserModel Create(long uid, string? firstName, string? phone, string state)
    {
        return new UserModel
        {
            UId = uid,
            FirstName = firstName,
            Phone = phone,
            UserState = state,
        };
    }
    
}