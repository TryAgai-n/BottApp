using BottApp.Database.User;
using Microsoft.Extensions.Logging;

namespace BottApp.Database;

public class DatabaseContainer : IDatabaseContainer
{
    public IUserRepository UserRepository { get; }

    public DatabaseContainer(PostgreSqlContext db, ILoggerFactory loggerFactory)
    {
        UserRepository = new UserRepository(db, loggerFactory);
    }
}