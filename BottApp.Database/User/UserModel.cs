using BottApp.Model.User;

namespace BottApp.Database.User;

public class UserModel : AbstractModel, IUser
{
    public long Id { get; set; }

    public string FirstName { get; set; }
    public string Phone { get; set; }

    public bool IsSendContact { get; set; }
}