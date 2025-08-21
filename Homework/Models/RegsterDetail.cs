namespace Homework.Models
{
    public class RegsterDetail
    {
            public int UserDetailId { get; set; }

            public int StudentId { get; set; }

            public string Username { get; set; } = null!;

            public string Password { get; set; } = null!;

            public string? Email { get; set; }

            public string? Phone { get; set; }
        }
}
