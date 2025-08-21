using Homework.Helper;
using Homework.Repository.Models;
using Homework.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols;

namespace Homework.Services
{
    public class AuthService : IAuthService
    {
        private readonly TestDbContext _context;
        private readonly JwtHelper _jwtHelper;
        private readonly IConfiguration _config;


        public AuthService(TestDbContext context,  IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public AuthenticateResponse Login(string username, string password)
        {
            var user = _context.UserDetails.SingleOrDefault(item => item.Username == username);
            if (user == null)
            {
               return new AuthenticateResponse(null, null) { Message = "Username could not be found" };
            }
            bool isPasswordValid = PasswordHasher.VerifyPassword(password, user.PasswordHash);

            if (!isPasswordValid)
            {
                return new AuthenticateResponse(null, null) { Message = "Username & password didn't match" };
            }
            var jwt = new JwtHelper(_config);
            var token = jwt.GenerateToken(username);
            return new AuthenticateResponse(user, token);
        }

        public async Task<bool> Register(UserDetail userDetail)
        {
            var password1 = PasswordHasher.HashPassword(userDetail.PasswordHash);
            var user = new UserDetail
            {
                Email = userDetail.Email,
                PasswordHash = password1,
                Phone = userDetail.Phone,
                Username = userDetail.Username,
                StudentId = userDetail.StudentId,
            };
            var userAdd = _context.UserDetails.Add(user);
            if (userAdd == null) return false;

            _context.SaveChanges();

           return true;
        }
    }
}
