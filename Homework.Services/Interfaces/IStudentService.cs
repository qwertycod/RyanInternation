using Homework.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homework.Services.Interfaces
{
    public interface IStudentService
    {
        Task<List<Student>> GetStudentsAsync();
    }
}
