namespace BottApp.Model.User;

public interface IUser
{
    long Id { get; }

    string Name { get; }

    string Phone { get; }

    bool IsSendContact { get; }
}