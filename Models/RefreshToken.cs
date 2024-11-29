namespace TourneyAPI.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public required string Token { get; set; }
        public DateTime Expires { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsExpired => DateTime.UtcNow >= Expires;
        public bool IsActive => !IsExpired;
        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = null!;
    }
}