using Xunit;
using TourneyAPI.Services;
using TourneyAPI.Models;
using TourneyAPI.Data;
using Microsoft.EntityFrameworkCore;

using Xunit.Abstractions;

namespace TourneyAPI.Tests
{
    public class SingleEliminationGeneratorTests
    {
        private readonly SingleEliminationGenerator _generator;
        private readonly ITestOutputHelper _output;
        private readonly TournamentContext _context;

        public SingleEliminationGeneratorTests(ITestOutputHelper output)
        {
            _output = output;
            var options = new DbContextOptionsBuilder<TournamentContext>()
                .UseInMemoryDatabase(databaseName: "TournamentDb")
                .Options;
            _context = new TournamentContext(options);
            _generator = new SingleEliminationGenerator();
        }

        private void PrintBracket(IList<TourneyAPI.Models.Match> matches, Models.Match? invalidMatch)
        {
            var rounds = matches.GroupBy(m => m.RoundNumber)
                              .OrderBy(g => g.Key);

            foreach (var round in rounds)
            {
                _output.WriteLine($"\nРаунд {round.Key}:");
                foreach (var match in round)
                {
                    _output.WriteLine($"Матч {match.Id} - {match.Team1Id} vs {match.Team2Id}");
                }
            }
            if (invalidMatch != null)
            {
                _output.WriteLine($"Неполный матч {invalidMatch.Id} Раунд: {invalidMatch.RoundNumber}");
            }
        }

        [Fact]
        public void CreateBracket_WithFourTeams_CreatesThreeMatches()
        {
            // Arrange 
            var teams = new List<Team>
            {
                new() {  Name = "Team 1" },
                new() {  Name = "Team 2" },
                new() {  Name = "Team 3" },
                new() {  Name = "Team 4" }
            };
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
            _context.Teams.AddRange(teams);
            // Act
            var matches = _generator.CreateBracket(teams);
            _context.Matches.AddRange(matches);
            var invalidMatch = _generator.ValidateBracket(matches);
            _context.SaveChanges();

            PrintBracket(matches, invalidMatch);
            // Assert
            Assert.Equal(3, matches.Count);
            Assert.Contains(matches, m => m.RoundNumber == 2);
            Assert.Equal(2, matches.Count(m => m.RoundNumber == 1));
        }

        [Fact]
        public void CreateBracket_WithFiveTeams_CreatesCorrectStructure()
        {
            // Arrange
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
            var teams = new List<Team>
            {
                new() { Name = "Team 1" },
                new() { Name = "Team 2" },
                new() { Name = "Team 3" },
                new() { Name = "Team 4" },
                new() { Name = "Team 5" }
            };
            _context.Teams.AddRange(teams);
            // Act
            var matches = _generator.CreateBracket(teams);
            var invalidMatch = _generator.ValidateBracket(matches);
            _context.Matches.AddRange(matches);

            PrintBracket(matches, invalidMatch); // Для визуального контроля

            // Assert
            Assert.Equal(4, matches.Count); // Общее количество матчей

            // Проверка структуры раундов
            var roundGroups = matches.GroupBy(m => m.RoundNumber)
                                    .OrderBy(g => g.Key)
                                    .ToList();
            Assert.Equal(3, roundGroups.Count); // Должно быть 3 раунда

            // Проверка первого раунда
            var firstRound = roundGroups.First().ToList();
            Assert.Equal(2, firstRound.Count); // 4 матча в первом раунде
        }

        [Fact]
        public void CreateBracket_WithSevenTeams_CreatesCorrectStructure()
        {
            // Arrange
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
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
            _context.Teams.AddRange(teams);
            // Act
            var matches = _generator.CreateBracket(teams);
            var invalidMatch = _generator.ValidateBracket(matches);
            _context.Matches.AddRange(matches);
            _context.SaveChanges();

            PrintBracket(matches, invalidMatch); // Для визуального контроля

            // Assert
            Assert.Equal(6, matches.Count); // Общее количество матчей

            // Проверка структуры раундов
            var roundGroups = matches.GroupBy(m => m.RoundNumber)
                                    .OrderBy(g => g.Key)
                                    .ToList();
            Assert.Equal(3, roundGroups.Count); // Должно быть 3 раунда

            // Проверка первого раунда
            var firstRound = roundGroups.First().ToList();
            Assert.Equal(3, firstRound.Count); // 4 матча в первом раунде
        }

        [Fact]
        public void CreateBracket_WithNineTeams_CreatesCorrectStructure()
        {
            // Arrange
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
            var teams = new List<Team>
            {
                new() { Name = "Team 1" },
                new() { Name = "Team 2" },
                new() { Name = "Team 3" },
                new() { Name = "Team 4" },
                new() { Name = "Team 5" },
                new() { Name = "Team 6" },
                new() { Name = "Team 7" },
                new() { Name = "Team 8" },
                new() { Name = "Team 9" }
            };

            // Act
            var matches = _generator.CreateBracket(teams);
            var invalidMatch = _generator.ValidateBracket(matches);
            _context.Matches.AddRange(matches);
            _context.SaveChanges();

            PrintBracket(matches, invalidMatch); // Для визуального контроля

            // Assert
            Assert.Equal(8, matches.Count); // Общее количество матчей

            // Проверка структуры раундов
            var roundGroups = matches.GroupBy(m => m.RoundNumber)
                                    .OrderBy(g => g.Key)
                                    .ToList();
            Assert.Equal(4, roundGroups.Count); // Должно быть 3 раунда

            // Проверка первого раунда
            var firstRound = roundGroups.First().ToList();
            Assert.Equal(4, firstRound.Count); // 4 матча в первом раунде
        }
    }
}