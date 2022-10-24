using BottApp.Data.Book;
using BottApp.Data.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BottApp.Data
{
    public interface IDatabaseContainer
    {
        IUserRepository User { get; }
        IBookRepository Book { get; }
    }
}
