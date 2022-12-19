using BottApp.Database.Document;

namespace BottApp.Database.User;

public class LocalUser
{
    public int Id { get; set; }
    public InNomination Nomination;
        
    public  bool IsSendCaption { get; set; }
    public  bool IsSendDocument { get; set; }
    public  bool IsSendNomination { get; set; }
    
    public string DocumentCaption { get; set; }
        
    public LocalUser(int id, InNomination nomination)
    {
        Id = id;
        Nomination = nomination;
    }
}