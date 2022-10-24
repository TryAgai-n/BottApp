using BottApp.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BottApp.Data.Book
{
    public interface IBookRepository
    {
        Task<BookModel> CreateBook(string title, string description);

    }
}
