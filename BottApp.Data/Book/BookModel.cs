using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BottApp.Data.Book
{
    public class BookModel : AbstractModel
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }


        public static BookModel Create(string title, string description)
        {
            return new BookModel
            {
                Title = title,
                Description = description
            };
        }

    }
}
