using TourneyAPI.Models;
using TourneyAPI.Models.DTOs;

namespace TourneyAPI.Extensions
{
    public static class MappingExtensions
    {
        public static TournamentDto ToDto(this Tournament tournament, string? currentUserId, bool isAdmin)
        {
            return new TournamentDto(
                Id: tournament.Id,
                Name: tournament.Name,
                StartDate: tournament.StartDate,
                EndDate: tournament.EndDate,
                TournamentStatus: tournament.TournamentStatus,
                Matches: tournament.Matches.Select(m => new MatchDto(
                    Id: m.Id,
                    RoundNumber: m.RoundNumber,
                    Team1Id: m.Team1Id,
                    Team2Id: m.Team2Id,
                    Team1Name: m.Team1?.Name,
                    Team2Name: m.Team2?.Name,
                    Status: m.Status,
                    NextMatchId: m.NextMatchId
                )).ToList(),
                CanEdit: currentUserId != null && tournament.CanEdit(currentUserId, isAdmin)
            );
        }
    }

};