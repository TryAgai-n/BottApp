using BottApp.Database.User;

namespace BottApp.Database
{
    public interface IDatabaseContainer
    {
        IUserRepository User { get; }
    }
}
