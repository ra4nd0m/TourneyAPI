using TourneyAPI.Models;
using TourneyAPI.Data;
using TourneyAPI.Models.DTOs;
using TourneyAPI.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace TourneyAPI.Services
{
    public class TournamentService(TournamentContext context) : ITournamentService
    {
        public async Task<Tournament> CreateTournament(CreateTournamentDto createTournamentDto, string adminId)
        {
            List<Team> teams = createTournamentDto.Teams;
            List<TournamentTeam> tournamentTeams = [];
            foreach (var newTeam in teams)
            {
                var existingTeam = await context.Teams.FirstOrDefaultAsync(t => t.Name == newTeam.Name);
                if (existingTeam == null)
                {
                    existingTeam = new Team { Name = newTeam.Name };
                    context.Teams.Add(existingTeam);
                    await context.SaveChangesAsync();
                }
                tournamentTeams.Add(new TournamentTeam { Team = existingTeam });
            }

            Tournament tournament = new Tournament
            {
                Name = createTournamentDto.Name,
                StartDate = createTournamentDto.StartDate.ToUniversalTime(),
                EndDate = createTournamentDto.EndDate.ToUniversalTime(),
                TournamentStatus = TournamentStatus.Created,
                AdminId = adminId,
                Teams = tournamentTeams
            };
            var generator = new SingleEliminationGenerator();
            tournament.Matches = generator.CreateBracket(tournamentTeams.Select(tt => tt.Team!).ToList());
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

        public async Task DeleteTournament(int tournamentId)
        {
            var tournament = await context.Tournaments
                .Include(t => t.Matches)
                .Include(t => t.Teams)
                .FirstOrDefaultAsync(t => t.Id == tournamentId) ?? throw new Exception("Tournament not found");
            context.Matches.RemoveRange(tournament.Matches);
            context.Tournaments.Remove(tournament);
            await context.SaveChangesAsync();
        }
    }
}