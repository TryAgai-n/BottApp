using BottApp.Model.User;

namespace BottApp.Database.User;

public class UserModel : AbstractModel, IUser
{
    public long Id { get; set; }

    public string FirstName { get; set; }

    public string Phone { get; set; }

    public bool IsSendContact { get; set; }

    public UserModel(long id, string firstName, string phone, bool isSendContact)
    {
        Id = id;
        FirstName = firstName;
        Phone = phone;
        IsSendContact = isSendContact;
    }
}