using Homework.Helper;
using Homework.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homework.Services.Interfaces
{
    public interface IAuthService
    {
        AuthenticateResponse Login(string username, string password);

        Task<bool> Register(UserDetail userDetail);
    }
}
