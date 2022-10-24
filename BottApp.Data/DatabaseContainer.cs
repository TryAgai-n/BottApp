using BottApp.Data.Book;
using BottApp.Data.User;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BottApp.Data
{

    public class DatabaseContainer : IDatabaseContainer
    {
        public IUserRepository User { get; }

        public IBookRepository Book { get; }

        public DatabaseContainer(PostgreSqlContext db, ILoggerFactory loggerFactory)
        {
            User = new UserRepository(db, loggerFactory);
            Book = new BookRepository(db, loggerFactory);
        }


    }
    
}
