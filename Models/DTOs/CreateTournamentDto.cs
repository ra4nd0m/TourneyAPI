namespace TourneyAPI.Models.DTOs
{
    public record CreateTournamentDto(
        string Name,
        DateTime StartDate,
        DateTime EndDate,
        List<Team> Teams
    );
}