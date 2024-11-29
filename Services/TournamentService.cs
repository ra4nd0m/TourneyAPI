using TourneyAPI.Models;
using TourneyAPI.Data;
using TourneyAPI.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace TourneyAPI.Services
{
    public class TournamentService(TournamentContext context) : ITournamentService
    {
        public async Task<Tournament> CreateTournament(string name, List<Team> teams, string adminId)
        {
            var teamNames = teams.Select(t => t.Name).ToList();
            var existingTeams = await context.Teams
                .Where(t => teamNames.Contains(t.Name))
                .ToDictionaryAsync(t => t.Name, t => t);
            var tournamentTeams = teams.Select(newTeam =>
            {
                var team = existingTeams.GetValueOrDefault(newTeam.Name) ?? newTeam;
                return new TournamentTeam { Team = team };
            }).ToList();

            Tournament tournament = new Tournament
            {
                Name = name,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(7),
                TournamentStatus = TournamentStatus.Created,
                AdminId = adminId,
                Teams = tournamentTeams
            };
            var generator = new SingleEliminationGenerator();
            tournament.Matches = generator.CreateBracket(teams);
            context.Tournaments.Add(tournament);
            await context.SaveChangesAsync();
            return tournament;
        }

        public async Task<List<Tournament>> GetTournaments()
        {
            return await context.Tournaments
                .Include(t => t.Teams)
                .Include(t => t.Matches)
                    .ThenInclude(m => m.Team1)
                .Include(t => t.Matches)
                    .ThenInclude(m => m.Team2)
                .ToListAsync();
        }

        public async Task<Tournament> GetTournament(int id)
        {
            var tournament = await context.Tournaments
                .Include(t => t.Teams)
                .Include(t => t.Matches)
                    .ThenInclude(m => m.Team1)
                .Include(t => t.Matches)
                    .ThenInclude(m => m.Team2)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (tournament == null)
                throw new Exception("Tournament not found");
            return tournament;
        }

        public async Task<Tournament> UpdateTournamentStatus(int id, TournamentStatus status)
        {
            var tournament = await context.Tournaments
                .FirstOrDefaultAsync(t => t.Id == id);
            if (tournament == null)
                throw new Exception("Tournament not found");
            tournament.TournamentStatus = status;
            if (status == TournamentStatus.Completed)
            {
                var finalMatch = tournament.Matches
                    .OrderByDescending(m => m.RoundNumber)
                    .FirstOrDefault();
                if (finalMatch?.Result?.WinnerId == null)
                    throw new Exception("Cannot complete tournament: final match has no winner");
                tournament.WinnerTeamId = finalMatch.Result.WinnerId;
            }
            await context.SaveChangesAsync();
            return tournament;
        }
    }
}