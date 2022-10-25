using BottApp.Data.User;

namespace BottApp.Data
{
    public interface IDatabaseContainer
    {
        IUserRepository User { get; }
    }
}
