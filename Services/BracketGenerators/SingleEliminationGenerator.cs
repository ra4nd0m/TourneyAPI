using TourneyAPI.Models;
using TourneyAPI.Interfaces;
using TourneyAPI.Data;

namespace TourneyAPI.Services
{
    public class SingleEliminationGenerator : ITournamentGenerator
    {
        public List<Match> CreateBracket(List<Team> teams)
        {
            List<Match> matches = new List<Match>();
            int teamCount = teams.Count;
            int maxDepth = (int)Math.Ceiling(Math.Log2(teamCount));
            int totalMatches = teamCount - 1;

            Match finalMatch = new Match
            {
                NextMatch = null,
                RoundNumber = maxDepth,
                Status = MatchStatus.Scheduled
            };
            void AddMatchRecursive(Match? nextMatch, int currentDepth)
            {
                if (matches.Count >= totalMatches)
                {
                    return;
                }

                Match match = new Match
                {
                    NextMatch = nextMatch,
                    RoundNumber = maxDepth - currentDepth,
                    Status = MatchStatus.Scheduled
                };
                matches.Add(match);

                if (currentDepth + 1 < maxDepth)
                {
                    AddMatchRecursive(match, currentDepth + 1);
                    AddMatchRecursive(match, currentDepth + 1);
                }
            };
            AddMatchRecursive(null, 0);
            AssignTeamsToMatches(matches, teams);
            return matches;
        }
        private void AssignTeamsToMatches(List<Match> matches, List<Team> teams)
        {
            int currentTeam = 0;
            foreach (Match match in matches.Where(m => m.RoundNumber == 1))
            {
                match.Team1 = teams[currentTeam];
                currentTeam++;
                if (currentTeam < teams.Count)
                {
                    match.Team2 = teams[currentTeam];
                    currentTeam++;
                }
            }
            if (currentTeam < teams.Count)
            {
                var invalidMatch = ValidateBracket(matches);
                if (invalidMatch != null)
                {
                    invalidMatch.Team2 = teams[currentTeam];
                }
            }

        }
        public Match? ValidateBracket(List<Match> matches)
        {
            // Словарь для подсчета предыдущих матчей
            Dictionary<Match, int> previousMatchesCount = new Dictionary<Match, int>();

            // Подсчитываем количество предыдущих матчей
            foreach (var match in matches)
            {
                if (match.NextMatch != null)
                {
                    if (!previousMatchesCount.ContainsKey(match.NextMatch))
                    {
                        previousMatchesCount[match.NextMatch] = 0;
                    }
                    previousMatchesCount[match.NextMatch]++;
                }
            }

            // Проверяем что у каждого матча (кроме первого раунда) есть 2 предыдущих
            foreach (var match in matches)
            {
                if (match.RoundNumber > 1)
                {
                    if (!previousMatchesCount.ContainsKey(match) ||
                        previousMatchesCount[match] != 2)
                    {
                        return match;
                    }
                }
            }
            return null;
        }
    }
}
