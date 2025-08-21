using Homework.Repository.Models;
using Homework.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Homework.Services
{
    public class StudentService : IStudentService
    {
        private readonly TestDbContext _context;

        public StudentService(TestDbContext context)
        {
            _context = context;
        }

        public async Task<List<Student>> GetStudentsAsync()
        {
            return await _context.Students.ToListAsync();
        }
    }
}
