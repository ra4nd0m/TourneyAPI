namespace TourneyAPI.Models
{
    public class TournamentTeam
    {
        public int TournamentId { get; set; }
        public int TeamId { get; set; }
        public int Seed { get; set; }
        public Tournament? Tournament { get; set; }
        public Team? Team { get; set; }
    }
}