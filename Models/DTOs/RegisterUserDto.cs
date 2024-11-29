namespace TourneyAPI.Models.DTOs
{
    public record RegisterUserDto(
        string UserName,
        string Email,
        string Password
    );
}