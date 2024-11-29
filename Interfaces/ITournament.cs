using TourneyAPI.Models;

namespace TourneyAPI.Interfaces
{
    public interface ITournamentService
    {
        Task<Tournament> CreateTournament(string name, List<Team> teams, string adminId);
        Task<List<Tournament>> GetTournaments();
        Task<Tournament> GetTournament(int id);
        //   Task<List<Match>> GetNextRoundMatches(int TournamentId);
    }

    public interface ITournamentGenerator
    {
        List<Match> CreateBracket(List<Team> teams);
        Match? ValidateBracket(List<Match> matches);
    }
}