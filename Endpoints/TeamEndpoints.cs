using TourneyAPI.Models;
using TourneyAPI.Models.DTOs;
using TourneyAPI.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace TourneyAPI.Endpoints
{
    public static class TeamEndpoints
    {
        public static void MapTeamEndpoints(this WebApplication app)
        {
            var teamEndpoints = app.MapGroup("/api/teams");
            teamEndpoints.MapGet("/", async (ITeamService teamService) =>
            {
                try
                {
                    var teams = await teamService.GetTeams();
                    return Results.Ok(teams);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex.Message);
                }
            });
            teamEndpoints.MapGet("/{id}", async (ITeamService teamService, int id) =>
            {
                try
                {
                    var team = await teamService.GetTeam(id);
                    return team == null ? Results.NotFound() : Results.Ok(team);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex.Message);
                }
            });
            teamEndpoints.MapPost("/", [Authorize(Policy = "AdminOrUser")] async (ITeamService teamService, TeamDto teamDto) =>
            {
                try
                {
                    var team = await teamService.CreateTeam(teamDto);
                    return Results.Created($"/api/teams/{team.Id}", team);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex.Message);
                }
            });
            teamEndpoints.MapPut("/{id}", [Authorize(Policy = "AdminOrUser")] async (ITeamService teamService, int id, TeamDto teamDto) =>
            {
                try
                {
                    var team = await teamService.UpdateTeam(id, teamDto);
                    return team == null ? Results.NotFound() : Results.Ok(team);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex.Message);
                }
            });
        }
    }
}