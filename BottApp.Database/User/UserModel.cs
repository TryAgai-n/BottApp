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
    
    public string? TelegramLastName { get; set; }
    
    public string? Phone { get; set; }

    
    public TelegramProfile? TelegramProfile
    {
        get => new TelegramProfile(UId, TelegramFirstName, TelegramLastName, Phone);
    } 
    
    [Required]
    public OnState OnState { get; set; }
    
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    
    
    public List<MessageModel> Messages { get; set; }
    public List<DocumentModel> Documents { get; set; }

    [Required]

    public InNomination? InNomination { get; set; }


    public static UserModel Create(TelegramProfile telegramProfile)
    {
        return new UserModel
        {
            UId = telegramProfile.UId,
            TelegramFirstName = telegramProfile.TelegramFirstName,
            TelegramLastName = telegramProfile.TelegramLastName,
            Phone = telegramProfile.Phone,
            InNomination = Document.InNomination.ㅤ,
            OnState = OnState.Auth,
            Messages = new List<MessageModel>(),
            Documents = new List<DocumentModel>()
        };
    }



    public void SetUserProfile(Profile profile)
    {
        FirstName = profile.FirstName;
        LastName = profile.LastName;
    }
    
}