using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TourneyAPI.Data;
using TourneyAPI.Models;

namespace TourneyAPI.Services
{
    public class JwtService(IConfiguration configuration, UserManager<User> userManager, TournamentContext context)
    {
        public async Task<string> GenerateToken(User user)
        {
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]!));
            var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var roles = await userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.UserName!),
                new(ClaimTypes.Email, user.Email!),
                new(ClaimTypes.NameIdentifier, user.Id)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: signingCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<RefreshToken> GenerateRefreshToken(User user)
        {
            RefreshToken token = new()
            {
                UserId = user.Id,
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.UtcNow.AddHours(4),
            };
            await context.RefreshTokens.AddAsync(token);
            await context.SaveChangesAsync();
            return token;
        }

        public async Task<RefreshToken?> ValidateRefreshToken(string token)
        {
            var refreshToken = await context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);
            if (refreshToken == null || refreshToken.IsExpired)
            {
                return null;
            }
            return refreshToken;
        }
    }
}