using TourneyAPI.Models;
using TourneyAPI.Models.DTOs;

namespace TourneyAPI.Interfaces
{
    public interface ITeamService
    {
        Task<List<Team>> GetTeams();
        Task<Team> GetTeam(int teamId);
        Task<Team> CreateTeam(TeamDto teamDto);
        Task<Team> UpdateTeam(int teamId, TeamDto teamDto);
        Task DeleteTeam(int teamId);
    }
}