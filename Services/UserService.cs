using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;
using TourneyAPI.Interfaces;
using TourneyAPI.Models;
using TourneyAPI.Models.DTOs;

namespace TourneyAPI.Services
{
    public class UserService(UserManager<User> userManager, RoleManager<IdentityRole> roleManager) : IUserService
    {
        public async Task<(bool success, string message)> Register(RegisterUserDto user)
        {
            var newUser = new User { UserName = user.UserName, Email = user.Email };
            var result = await userManager.CreateAsync(newUser, user.Password);
            if (result.Succeeded)
            {
                if (!await roleManager.RoleExistsAsync("User"))
                {
                    await roleManager.CreateAsync(new IdentityRole("User"));
                }
                await userManager.AddToRoleAsync(newUser, "User");
                return (true, "User created successfully");
            }
            return (false, $"User creation failed, {result.Errors.Select(e => e.Description)}");
        }

        public async Task<(bool success, string message)> RegisterAdmin(RegisterUserDto model)
        {
            var user = new User { UserName = model.UserName, Email = model.Email };
            var result = await userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Admin");
                return (true, "Admin created successfully");
            }
            return (false, $"Admin creation failed, {result.Errors.Select(e => e.Description)}");
        }
        public Task<User> GetUser(int id)
        {
            throw new NotImplementedException();
        }
    }
}