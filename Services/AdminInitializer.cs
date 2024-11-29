using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TourneyAPI.Models;

namespace TourneyAPI.Services
{
    public static class AdminInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            if (!await userManager.Users.AnyAsync(u => u.Email == "admin@admin.com"))
            {
                User admin = new User
                {
                    UserName = "admin",
                    Email = "admin@admin.com"
                };

                var result = await userManager.CreateAsync(admin, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }
        }
    }
}