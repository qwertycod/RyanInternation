using Homework.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homework.Helper
{
    public class AuthenticateResponse
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string FirstName { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Token { get; set; }
        public string Message { get; set; }


        public AuthenticateResponse(UserDetail user, string token)
        {
            if (user != null)
            {
                Id = user.UserDetailId;
                StudentId = user.StudentId;
                FirstName = user.Username;
                Email = user.Email;
                Username = user.Username;
                Token = token;
            }
        }
    }
}