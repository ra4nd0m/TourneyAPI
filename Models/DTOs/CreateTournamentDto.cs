namespace TourneyAPI.Models.DTOs
{
    public record CreateTournamentDto(
        string Name,
        List<Team> Teams
    );
}