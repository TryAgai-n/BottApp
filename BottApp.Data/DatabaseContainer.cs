using BottApp.Data.User;
using Microsoft.Extensions.Logging;

namespace BottApp.Data
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
