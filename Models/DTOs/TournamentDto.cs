namespace TourneyAPI.Models.DTOs
{
    public record TournamentDto(
        int Id,
        string Name,
        DateTime StartDate,
        DateTime EndDate,
        TournamentStatus TournamentStatus,
        List<MatchDto> Matches,
        bool CanEdit
    );

    public record MatchDto(
        int Id,
        int RoundNumber,
        int? Team1Id,
        int? Team2Id,
        string? Team1Name,
        string? Team2Name,
        MatchStatus Status,
        int? NextMatchId
    );
}