using Homework.Repository.Models;

namespace Homework.Models
{
    public class StudentBookDto
    {
        public int StudentId { get; set; }

        public int BookId { get; set; }

        public DateTime? PurchaseDate { get; set; }

        public int UpdateId { get; set; }
    }
}
