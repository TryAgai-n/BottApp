namespace BottApp.Database.Models;
public interface IUser
{
    int Id { get; }

    string FirstName { get; }

    string Phone { get; }

    bool IsSendContact { get; }
}