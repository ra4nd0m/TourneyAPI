using TourneyAPI.Models;
using TourneyAPI.Data;
using TourneyAPI.Interfaces;
using Microsoft.EntityFrameworkCore;
using TourneyAPI.Models.DTOs;

namespace TourneyAPI.Services
{
    public class TeamService(TournamentContext context) : ITeamService
    {
        public async Task<List<Team>> GetTeams()
        {
            List<Team> teams = await context.Teams.ToListAsync();
            return teams;
        }

        public async Task<Team> GetTeam(int teamId)
        {
            Team? team = await context.Teams.FirstOrDefaultAsync(t => t.Id == teamId);
            if (team == null)
                throw new Exception("Team not found");
            return team;
        }

        public async Task<Team> CreateTeam(TeamDto teamDto)
        {
            var team = new Team() { Name = teamDto.TeamName };
            context.Teams.Add(team);
            await context.SaveChangesAsync();
            return team;
        }

        public async Task<Team> UpdateTeam(int teamId, TeamDto teamDto)
        {
            Team? existingTeam = await context.Teams.FirstOrDefaultAsync(t => t.Id == teamId);
            if (existingTeam == null)
                throw new Exception("Team not found");
            existingTeam.Name = teamDto.TeamName;
            await context.SaveChangesAsync();
            return existingTeam;
        }

        public async Task DeleteTeam(int teamId)
        {
            Team? team = await context.Teams.FirstOrDefaultAsync(t => t.Id == teamId);
            if (team == null)
                throw new Exception("Team not found");
            context.Teams.Remove(team);
            await context.SaveChangesAsync();
        }
    }
}