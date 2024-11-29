using TourneyAPI.Models;
using TourneyAPI.Models.DTOs;

namespace TourneyAPI.Interfaces
{
    public interface IUserService
    {
        Task<(bool success, string message)> Register(RegisterUserDto user);
        Task<(bool success, string message)> RegisterAdmin(RegisterUserDto user);
        Task<User> GetUser(int id);
    }
}