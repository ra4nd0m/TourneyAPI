namespace TourneyAPI.Models.DTOs
{
    public record LoginUserDto(
        string Email,
        string Password
    );
}