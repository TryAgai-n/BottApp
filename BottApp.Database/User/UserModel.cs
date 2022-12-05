using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BottApp.Database.Document;
using BottApp.Database.UserMessage;
using Microsoft.EntityFrameworkCore;

namespace BottApp.Database.User;

public class UserModel : AbstractModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Required]
    public long UId { get; set; }
    public string? TelegramFirstName { get; set; }
    public string? Phone { get; set; }
    
    [Required]
    public OnState OnState { get; set; }
    
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    public List<MessageModel> Messages { get; set; }
    public List<DocumentModel> Documents { get; set; }

    
    public DocumentNomination? Nomination { get; set; }


    public static UserModel Create(long uid, string? telegramFirstName, string? phone)
    {
        return new UserModel
        {
            UId = uid,
            TelegramFirstName = telegramFirstName,
            Phone = phone,
            OnState = OnState.Auth,
            Messages = new List<MessageModel>(),
            Documents = new List<DocumentModel>()
        };
    }
}