namespace TourneyAPI.Models
{
    public class Tournament
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TournamentStatus TournamentStatus { get; set; }
        public string AdminId { get; set; } = null!;
        public User Admin { get; set; } = null!;
        public int? WinnerTeamId { get; set; }
        public Team? WinnerTeam { get; set; }
        public List<Match> Matches { get; set; } = new List<Match>();
        public List<TournamentTeam> Teams { get; set; } = new List<TournamentTeam>();
        public bool CanEdit(string userId, bool isAdmin)
        {
            return userId == AdminId || isAdmin;
        }
    }
}