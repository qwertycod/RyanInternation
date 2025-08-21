using Homework.Repository.Interfaces;
using Homework.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homework.Repository
{
    public class BookRepository : IBookRepository
    {

        private readonly TestDbContext _context;
        public BookRepository(TestDbContext context) {
            _context = context;
        }

        public IEnumerable<Book> GetBooks()
        {
            {
                return _context.Books;
            }
        }
    }
}
