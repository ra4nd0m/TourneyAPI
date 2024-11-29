using Microsoft.AspNetCore.Identity;

namespace TourneyAPI.Models
{
    public class User : IdentityUser
    {
        public ICollection<Tournament> AdminTournaments { get; set; } = [];
        public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    }
}