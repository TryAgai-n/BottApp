namespace BottApp.Model.User;

public interface IUser
{
    long Id { get; }

    string FirstName { get; }

    string Phone { get; }

    bool IsSendContact { get; }
}