using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using TourneyAPI.Interfaces;
using TourneyAPI.Models;
using TourneyAPI.Models.DTOs;
using TourneyAPI.Services;

namespace TourneyAPI.Endpoints
{
    public static class UserEndpoints
    {
        public static void MapUserEndpoints(this WebApplication app)
        {
            var userEndpoints = app.MapGroup("/api/users");
            userEndpoints.MapPost("/register", async (IUserService userService, RegisterUserDto user) =>
            {
                try
                {
                    var (success, message) = await userService.Register(user);
                    if (success)
                    {
                        return Results.Created();
                    }
                    else
                    {
                        return Results.BadRequest(message);
                    }
                }
                catch
                {
                    return Results.StatusCode(StatusCodes.Status500InternalServerError);
                };
            });
            userEndpoints.MapPost("/login", async (JwtService jwtService, UserManager<User> userManager,
                LoginUserDto userModel, HttpContext context) =>
            {
                var user = await userManager.FindByEmailAsync(userModel.Email);
                if (user == null)
                {
                    return Results.BadRequest("Invalid email or password");
                }
                bool isValid = await userManager.CheckPasswordAsync(user, userModel.Password);
                if (!isValid)
                {
                    return Results.BadRequest("Invalid email or password");
                }
                string token = await jwtService.GenerateToken(user);
                RefreshToken refreshToken = await jwtService.GenerateRefreshToken(user);
                context.Response.Cookies.Append("refreshToken", refreshToken.Token, new CookieOptions
                {
                    HttpOnly = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = refreshToken.Expires
                });
                return Results.Ok(new { Token = token });
            });
            userEndpoints.MapPost("/register-admin", [Authorize(Policy = "Admin")] async (IUserService userService, RegisterUserDto user) =>
            {
                try
                {
                    var (success, message) = await userService.RegisterAdmin(user);
                    if (success)
                    {
                        return Results.Created();
                    }
                    else
                    {
                        return Results.BadRequest(message);
                    }
                }
                catch
                {
                    return Results.StatusCode(StatusCodes.Status500InternalServerError);
                };
            });
            userEndpoints.MapPost("/refresh", async (HttpContext context, JwtService jwtService, UserManager<User> userManager) =>
            {
                try
                {
                    if (context.Request.Cookies.TryGetValue("refreshToken", out string? token))
                    {
                        var refreshToken = await jwtService.ValidateRefreshToken(token);
                        if (refreshToken != null)
                        {
                            var user = await userManager.FindByIdAsync(refreshToken.UserId);
                            if (user != null)
                            {
                                var newToken = await jwtService.GenerateToken(user);

                                return Results.Ok(new { Token = newToken });
                            }
                        }
                    }
                    return Results.Unauthorized();
                }
                catch (Exception ex)
                {
                    return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
                }


            });
        }
    }
}