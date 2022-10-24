using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BottApp.Data.Book
{

    internal class BookRepository : AbstractRepository<BookModel>, IBookRepository
    {
        public BookRepository(PostgreSqlContext context, ILoggerFactory loggerFactory) : base(context, loggerFactory)
        {
        }

        public async Task<BookModel> CreateBook(string title, string description)
        {
            var model = BookModel.Create(title, description);

            var result = await CreateModelAsync(model);
            if(result==null)
            {
                throw new Exception("A book model not created");
            }
            return result;

        }
    }
}
