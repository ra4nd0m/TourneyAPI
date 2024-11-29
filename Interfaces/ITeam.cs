using TourneyAPI.Models;

namespace TourneyAPI.Interfaces
{
    public interface ITeamService
    {
        Task<List<Team>> GetTeams();
        Task<Team> GetTeam(int teamId);
        Task<Team> CreateTeam(Team team);
        Task<Team> UpdateTeam(int teamId, Team team);
        Task DeleteTeam(int teamId);
    }
}