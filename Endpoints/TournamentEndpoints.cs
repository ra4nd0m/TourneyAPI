using TourneyAPI.Models.DTOs;
using TourneyAPI.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TourneyAPI.Extensions;

namespace TourneyAPI.Endpoints
{

    public static class TournamentEndpoints
    {
        public static void MapTournamentEndpoints(this WebApplication app)
        {
            RouteGroupBuilder tournamentEndpoints = app.MapGroup("/api/tournaments");
            tournamentEndpoints.MapGet("/{id}", [AllowAnonymous] async (ITournamentService tournamentService, int id, ClaimsPrincipal? user) =>
            {
                try
                {
                    var userId = user?.FindFirstValue(ClaimTypes.NameIdentifier);
                    bool isAdmin = user?.IsInRole("Admin") ?? false;
                    var result = await tournamentService.GetTournament(id);
                    return Results.Ok(result.ToDto(userId, isAdmin));
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex.Message);
                }
            });
            tournamentEndpoints.MapGet("/", [AllowAnonymous] async (ITournamentService tournamentService, ClaimsPrincipal? user) =>
            {
                try
                {
                    var tournaments = await tournamentService.GetTournaments();
                    var userId = user?.FindFirstValue(ClaimTypes.NameIdentifier);
                    bool isAdmin = user?.IsInRole("Admin") ?? false;
                    return Results.Ok(tournaments.Select(t => t.ToDto(userId, isAdmin)));
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex.Message);
                }
            });
            tournamentEndpoints.MapPost("/", [Authorize(Policy = "AdminOrUser")] async (ITournamentService tournamentService, CreateTournamentDto tournamentDto, ClaimsPrincipal user) =>
            {
                try
                {
                    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (string.IsNullOrEmpty(userId))
                    {
                        return Results.Unauthorized();
                    }

                    var result = await tournamentService.CreateTournament(tournamentDto.Name, tournamentDto.Teams, userId);
                    return Results.Ok(result.ToDto(userId, false));
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex.Message);
                }
            });
        }
    }
}