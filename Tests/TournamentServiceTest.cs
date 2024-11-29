using Microsoft.EntityFrameworkCore;
using TourneyAPI.Data;
using TourneyAPI.Models;
using TourneyAPI.Models.DTOs;
using TourneyAPI.Services;
using Xunit;
using Xunit.Abstractions;

namespace TourneyAPI.Tests
{
    public class TournamentServiceTests
    {
        private readonly ITestOutputHelper _output;
        private readonly TournamentContext _context;
        private readonly TournamentService _service;
        public TournamentServiceTests(ITestOutputHelper output)
        {
            _output = output;
            var options = new DbContextOptionsBuilder<TournamentContext>()
                .UseInMemoryDatabase(databaseName: "TournamentDb")
                .Options;
            _context = new TournamentContext(options);
            _service = new TournamentService(_context);
        }

        [Fact]
        public async Task CreateTournament_ShouldCreateTournamentWithTeams()
        {
            // Arrange
            var teams = new List<Team>
            {
                new() { Name = "Team 1" },
                new() { Name = "Team 2" },
                new() { Name = "Team 3" },
                new() { Name = "Team 4" },
                new() { Name = "Team 5" },
                new() { Name = "Team 6" },
                new() { Name = "Team 7" }
            };

            await _context.Teams.AddRangeAsync(teams);
            await _context.SaveChangesAsync();
            // Act
            Tournament createdTournament;
            createdTournament = await _service.CreateTournament(new CreateTournamentDto(Name: "Test Tournament", StartDate: DateTime.Parse("2023-01-01"), EndDate: DateTime.Parse("2023-01-02"), Teams: teams), "1");


            // Assert
            var tournament = await _context.Tournaments
                .Include(t => t.Teams)
                    .ThenInclude(tt => tt.Team)
                .Include(t => t.Matches)
                    .ThenInclude(m => m.NextMatch)
                .FirstOrDefaultAsync(t => t.Id == createdTournament.Id);

            Assert.NotNull(tournament);
            Assert.Equal("Test Tournament", tournament.Name);
            Assert.Equal(TournamentStatus.Created, tournament.TournamentStatus);
            Assert.Equal(7, tournament.Teams.Count);
            Assert.Equal(6, tournament.Matches.Count);


            //Вывод турнира
            _output.WriteLine("\n=== Информация о турнире ===");
            _output.WriteLine($"Название: {tournament.Name}");
            _output.WriteLine($"Статус: {tournament.TournamentStatus}");
            _output.WriteLine($"Дата начала: {tournament.StartDate}");
            _output.WriteLine($"Дата окончания: {tournament.EndDate}");

            _output.WriteLine("\n=== Команды участники ===");
            foreach (var team in tournament.Teams)
            {
                _output.WriteLine($"- {team.Team?.Name ?? "Unknown Team"}");
            }

            _output.WriteLine("\n=== Турнирная сетка ===");
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
                    _output.WriteLine($"Матч {match.Id}: {team1Name} vs {team2Name}");
                }
            }
        }

        [Fact]
        public async Task GetTournaments_ShouldReturnEmptyList_WhenNoTournamentsExist()
        {
            // Arrange
            await _context.Database.EnsureDeletedAsync();
            await _context.Database.EnsureCreatedAsync();

            // Act
            var tournaments = await _service.GetTournaments();

            // Assert
            Assert.Empty(tournaments);
        }

        [Fact]
        public async Task GetTournaments_ShouldReturnAllTournamentsWithTeamsAndMatches()
        {
            // Arrange
            await _context.Database.EnsureDeletedAsync();
            await _context.Database.EnsureCreatedAsync();

            var teams1 = new List<Team>
            {
                new() { Name = "Team 1" },
                new() { Name = "Team 2" }
            };
            var teams2 = new List<Team>
            {
                new() { Name = "Team 3" },
                new() { Name = "Team 4" }
            };

            await _context.Teams.AddRangeAsync(teams1);
            await _context.Teams.AddRangeAsync(teams2);
            await _context.SaveChangesAsync();

            await _service.CreateTournament(new CreateTournamentDto(Name: "Test Tournament", StartDate: DateTime.Parse("2023-01-01"), EndDate: DateTime.Parse("2023-01-02"), Teams: teams1), "1");
            await _service.CreateTournament(new CreateTournamentDto(Name: "Test Tournament", StartDate: DateTime.Parse("2023-01-01"), EndDate: DateTime.Parse("2023-01-02"), Teams: teams2), "1");

            // Act
            var tournaments = await _service.GetTournaments();

            // Assert
            Assert.Equal(2, tournaments.Count);
            Assert.All(tournaments, t =>
            {
                Assert.NotNull(t.Teams);
                Assert.NotNull(t.Matches);
                Assert.Equal(2, t.Teams.Count);
                Assert.Single(t.Matches);
            });

            // Output results
            _output.WriteLine("\n=== Загруженные турниры ===");
            foreach (var tournament in tournaments)
            {
                _output.WriteLine($"\nТурнир: {tournament.Name}");
                _output.WriteLine("Команды:");
                foreach (var tt in tournament.Teams)
                {
                    _output.WriteLine($"- {tt.Team?.Name}");
                }
                _output.WriteLine("Матчи:");
                foreach (var match in tournament.Matches)
                {
                    var team1Name = tournament.Teams
                        .FirstOrDefault(tt => tt.Team?.Id == match.Team1Id)?
                        .Team?.Name ?? "TBD";
                    var team2Name = tournament.Teams
                        .FirstOrDefault(tt => tt.Team?.Id == match.Team2Id)?
                        .Team?.Name ?? "TBD";
                    _output.WriteLine($"- Матч {match.Id}: {team1Name} vs {team2Name}");
                }
            }
        }


        [Fact]
        public async Task GetTournament_ShouldReturnTournamentWithTeamsAndMatches()
        {
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

            var createdTournament = await _service.CreateTournament(new CreateTournamentDto(Name: "Test Tournament", StartDate: DateTime.Parse("2023-01-01"), EndDate: DateTime.Parse("2023-01-02"), Teams: teams), "1");

            // Act
            var tournament = await _service.GetTournament(createdTournament.Id);

            // Assert
            Assert.NotNull(tournament);
            Assert.Equal("Test Tournament", tournament.Name);
            Assert.NotNull(tournament.Teams);
            Assert.Equal(4, tournament.Teams.Count);
            Assert.NotNull(tournament.Matches);
            Assert.Equal(3, tournament.Matches.Count);

            // Output tournament details
            _output.WriteLine("\n=== Полученный турнир ===");
            _output.WriteLine($"ID: {tournament.Id}");
            _output.WriteLine($"Название: {tournament.Name}");
            _output.WriteLine($"Статус: {tournament.TournamentStatus}");
            _output.WriteLine($"Количество команд: {tournament.Teams.Count}");
            _output.WriteLine($"Количество матчей: {tournament.Matches.Count}");
        }

        [Fact]
        public async Task GetTournament_ShouldThrowException_WhenTournamentNotFound()
        {
            // Arrange
            const int nonExistentId = 999;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.GetTournament(nonExistentId));

            Assert.Equal("Tournament not found", exception.Message);
        }

        [Fact]
        public async Task GetTournament_ShouldLoadNavigationProperties()
        {
            // Arrange
            var teams = new List<Team>
            {
                new() { Name = "Team 1" },
                new() { Name = "Team 2" }
            };

            await _context.Teams.AddRangeAsync(teams);
            await _context.SaveChangesAsync();

            var createdTournament = await _service.CreateTournament(new CreateTournamentDto(Name: "Test Tournament", StartDate: DateTime.Parse("2023-01-01"), EndDate: DateTime.Parse("2023-01-02"), Teams: teams), "1");

            // Act
            var tournament = await _service.GetTournament(createdTournament.Id);

            // Assert
            Assert.NotNull(tournament.Teams);
            Assert.NotNull(tournament.Matches);

            _output.WriteLine("\n=== Проверка навигационных свойств ===");
            _output.WriteLine("Команды:");
            foreach (var tt in tournament.Teams)
            {
                _output.WriteLine($"- {tt.Team?.Name ?? "Не загружено"}");
            }

            _output.WriteLine("\nМатчи:");
            foreach (var match in tournament.Matches)
            {
                var team1Id = match.Team1Id;
                var team2Id = match.Team2Id;
                _output.WriteLine($"- Матч {match.Id}: Team{team1Id} vs Team{team2Id}");
            }
        }
    }
}