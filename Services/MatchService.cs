using TourneyAPI.Data;
using TourneyAPI.Interfaces;
using TourneyAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace TourneyAPI.Services
{
    public class MatchService(TournamentContext context) : IMatchService
    {
        public async Task<List<Match>> GetMatches()
        {   
            List<Match> matches = await context.Matches
                .Include(m => m.Team1)
                .Include(m => m.Team2)
                .Include(m => m.Result)
                .ToListAsync();
            return matches;
        }

        public async Task<Match> UpdateMatchResult(int matchId, MatchResult result)
        {
            Match? match = await context.Matches
                .Include(m => m.Result)
                .FirstOrDefaultAsync(m => m.Id == matchId);
            if (match == null)
                throw new Exception("Match not found");
            match.Result = result;
            match.Status = MatchStatus.Completed;

            if (match.NextMatchId != null && result.WinnerId != null)
            {
                Match? nextMatch = await context.Matches
                    .FirstOrDefaultAsync(m => m.Id == match.NextMatchId);
                if (nextMatch == null)
                    throw new Exception("Next match not found");
                if (nextMatch.Team1Id == null)
                    nextMatch.Team1Id = result.WinnerId;
                else if (nextMatch.Team2Id == null)
                    nextMatch.Team2Id = result.WinnerId;
                else
                    throw new Exception("Next match is already full");
            }
            await context.SaveChangesAsync();
            return match;
        }
    }
}