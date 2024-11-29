using TourneyAPI.Models;
using TourneyAPI.Models.DTOs;

namespace TourneyAPI.Interfaces
{
    public interface ITournamentService
    {
        Task<Tournament> CreateTournament(CreateTournamentDto createTournamentDto, string adminId);
        Task<List<Tournament>> GetTournaments();
        Task<Tournament> GetTournament(int id);
        Task<Tournament> UpdateTournamentStatus(int id, TournamentStatus status);
        Task DeleteTournament(int id);
        //   Task<List<Match>> GetNextRoundMatches(int TournamentId);
    }

    public interface ITournamentGenerator
    {
        List<Match> CreateBracket(List<Team> teams);
        Match? ValidateBracket(List<Match> matches);
    }
}