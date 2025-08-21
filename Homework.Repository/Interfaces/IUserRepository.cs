using Homework.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homework.Repository.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<UserDetail>> GetAllUsersAsync();
        Task<UserDetail> GetByIdAsync(int id);
        Task<UserDetail> GetByUserNameAsync(string userName);
        Task<bool> AddRefreshToken(RefreshToken refreshToken);
    }
}
