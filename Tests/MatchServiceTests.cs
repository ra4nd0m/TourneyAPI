using Microsoft.EntityFrameworkCore;
using TourneyAPI.Data;
using TourneyAPI.Models;
using TourneyAPI.Models.DTOs;
using TourneyAPI.Services;
using Xunit;
using Xunit.Abstractions;


namespace TourneyAPI.Tests
{
    public class MatchServiceTests
    {
        private readonly ITestOutputHelper _output;
        private readonly TournamentContext _context;
        private readonly MatchService _matchService;
        private readonly TournamentService _tournamentService;
        public MatchServiceTests(ITestOutputHelper output)
        {
            _output = output;
            var options = new DbContextOptionsBuilder<TournamentContext>()
                .UseInMemoryDatabase(databaseName: "TournamentDb")
                .Options;
            _context = new TournamentContext(options);
            _matchService = new MatchService(_context);
            _tournamentService = new TournamentService(_context);
        }
        [Fact]
        public async Task UpdateMatchResult_ShouldUpdateMatchAndPropagateWinner()
        {
            await _context.Database.EnsureDeletedAsync();
            await _context.Database.EnsureCreatedAsync();
            // Arrange
            var teams = new List<Team>
            {
                new() { Name = "Team 1" },
                new() { Name = "Team 2" },
                new() { Name = "Team 3" },
                new() { Name = "Team 4" }
            };

            await _context.Teams.AddRangeAsync(teams);
            await _context.SaveChangesAsync();

            var tournament = await _tournamentService.CreateTournament(new CreateTournamentDto(Name: "Test Tournament", StartDate: DateTime.Parse("2023-01-01"), EndDate: DateTime.Parse("2023-01-02"), Teams: teams), "1");
            var firstMatch = tournament.Matches.First(m => m.RoundNumber == 1);
            var result = new MatchResult { WinnerId = firstMatch.Team1Id, Team1Score = 1, Team2Score = 0 };

            // Act
            var updatedMatch = await _matchService.UpdateMatchResult(firstMatch.Id, result);
            var nextMatch = await _context.Matches.FirstOrDefaultAsync(m => m.Id == firstMatch.NextMatchId);

            // Assert
            Assert.Equal(MatchStatus.Completed, updatedMatch.Status);
            Assert.NotNull(updatedMatch.Result);
            Assert.Equal(result.WinnerId, updatedMatch.Result.WinnerId);
            Assert.NotNull(nextMatch);
            Assert.Equal(result.WinnerId, nextMatch.Team1Id);
            _output.WriteLine("\n=== Обновленная турнирная сетка ===");

            var updatedTournament = await _context.Tournaments
                .Include(t => t.Matches)
                    .ThenInclude(m => m.Result)
                .Include(t => t.Teams)
                    .ThenInclude(tt => tt.Team)
                .FirstOrDefaultAsync(t => t.Id == tournament.Id);

            var roundGroups = tournament.Matches
                .OrderBy(m => m.RoundNumber)
                .GroupBy(m => m.RoundNumber);

            foreach (var round in roundGroups)
            {
                _output.WriteLine($"\nРаунд {round.Key}:");
                foreach (var match in round)
                {
                    string team1Name = teams.FirstOrDefault(t => t.Id == match.Team1Id)?.Name ?? "TBD";
                    string team2Name = teams.FirstOrDefault(t => t.Id == match.Team2Id)?.Name ?? "TBD";
                    string status = match.Status.ToString();
                    string winner = match.Result?.WinnerId != null
                        ? $", Победитель: {teams.FirstOrDefault(t => t.Id == match.Result.WinnerId)?.Name} Счет: {match.Result.Team1Score}:{match.Result.Team2Score}"
                        : "";

                    _output.WriteLine($"Матч {match.Id}: {team1Name} vs {team2Name} ({status}{winner})");
                }
            }
        }

        [Fact]
        public async Task UpdateMatchResult_ShouldThrowException_WhenMatchNotFound()
        {
            // Arrange
            await _context.Database.EnsureDeletedAsync();
            await _context.Database.EnsureCreatedAsync();
            var result = new MatchResult { WinnerId = 1 };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () =>
                await _matchService.UpdateMatchResult(999, result));
        }

        [Fact]
        public async Task UpdateMatchResult_ShouldThrowException_WhenNextMatchIsFull()
        {
            // Arrange
            await _context.Database.EnsureDeletedAsync();
            await _context.Database.EnsureCreatedAsync();
            var teams = new List<Team>
            {
                new() { Name = "Team 1" },
                new() { Name = "Team 2" },
                new() { Name = "Team 3" },
                new() { Name = "Team 4" }
            };

            await _context.Teams.AddRangeAsync(teams);
            await _context.SaveChangesAsync();

            var tournament = await _tournamentService.CreateTournament(new CreateTournamentDto(Name: "Test Tournament", StartDate: DateTime.Parse("2023-01-01"), EndDate: DateTime.Parse("2023-01-02"), Teams: teams), "1");

            var firstMatches = tournament.Matches.Where(m => m.RoundNumber == 1).ToList();
            var finalMatch = tournament.Matches.First(m => m.RoundNumber == 2);

            // Complete first match
            await _matchService.UpdateMatchResult(firstMatches[0].Id, new MatchResult { WinnerId = firstMatches[0].Team1Id });

            // Complete second match
            await _matchService.UpdateMatchResult(firstMatches[1].Id, new MatchResult { WinnerId = firstMatches[1].Team1Id });

            // Try to update first match again
            var result = new MatchResult { WinnerId = firstMatches[0].Team2Id };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () =>
                await _matchService.UpdateMatchResult(firstMatches[0].Id, result));
        }

        [Fact]
        public async Task GetMatches_ShouldReturnEmptyList_WhenNoMatchesExist()
        {
            //Arrange
            await _context.Database.EnsureDeletedAsync();
            await _context.Database.EnsureCreatedAsync();

            // Act
            var matches = await _matchService.GetMatches();

            // Assert
            Assert.Empty(matches);
        }

        [Fact]
        public async Task GetMatches_ShouldReturnAllMatchesWithResults()
        {
            // Arrange
            await _context.Database.EnsureDeletedAsync();
            await _context.Database.EnsureCreatedAsync();
            var match1 = new Match
            {
                Team1 = new() { Name = "Team 1" },
                Team2 = new() { Name = "Team 2" },
                RoundNumber = 1,
                Status = MatchStatus.Completed,
                Result = new MatchResult
                {
                    Team1Score = 2,
                    Team2Score = 1,
                    WinnerId = 1
                }
            };

            var match2 = new Match
            {
                Team1= new() { Name = "Team 3" },
                Team2= new() { Name = "Team 4" },
                RoundNumber = 1,
                Status = MatchStatus.Scheduled
            };

            await _context.Matches.AddRangeAsync(match1, match2);
            await _context.SaveChangesAsync();

            // Act
            var matches = await _matchService.GetMatches();

            // Assert
            Assert.Equal(2, matches.Count);
            Assert.Contains(matches, m => m.Result != null);
            Assert.Contains(matches, m => m.Result == null);

            // Output для проверки
            _output.WriteLine("\n=== Список матчей ===");
            foreach (var match in matches)
            {
                var team1Name = match.Team1?.Name ?? "TBD";
                var team2Name = match.Team2?.Name ?? "TBD";
                _output.WriteLine($"Матч {match.Id}: {team1Name} vs {team2Name}");
                _output.WriteLine($"Статус: {match.Status}");
                if (match.Result != null)
                {
                    _output.WriteLine($"Результат: {match.Result.Team1Score} : {match.Result.Team2Score}");
                    _output.WriteLine($"Победитель: Team{match.Result.WinnerId}");
                }
                _output.WriteLine("-------------------");
            }
        }
    }
}