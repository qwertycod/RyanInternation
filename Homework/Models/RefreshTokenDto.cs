namespace Homework.Models
{
    public class RefreshTokenDto
    {
        public string Id { get; set; } = null!;

        public string Token { get; set; } = null!;

        public string? Username { get; set; }

        public DateOnly? Expires { get; set; }

        public bool? IsRevoked { get; set; }

        public bool? IsUsed { get; set; }
    }
}
