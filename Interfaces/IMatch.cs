using TourneyAPI.Models;

public interface IMatchService
{
    Task<Match> UpdateMatchResult(int matchId, MatchResult result);
    Task<List<Match>> GetMatches();
}