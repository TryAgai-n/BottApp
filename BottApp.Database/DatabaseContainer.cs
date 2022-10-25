using BottApp.Database.User;
using Microsoft.Extensions.Logging;

namespace BottApp.Database
{

    public class DatabaseContainer : IDatabaseContainer
    {
        public IUserRepository User { get; }


        public DatabaseContainer(PostgreSqlContext db, ILoggerFactory loggerFactory)
        {
            User = new UserRepository(db, loggerFactory);
        }


    }
    
}
