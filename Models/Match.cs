namespace TourneyAPI.Models
{
    public class Match
    {
        public int Id { get; set; }
        public int TournamentId { get; set; }
        public int RoundNumber { get; set; }
        public int? Team1Id { get; set; }
        public Team? Team1 { get; set; }
        public int? Team2Id { get; set; }
        public Team? Team2 { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public Tournament? Tournament { get; set; }
        public int? NextMatchId { get; set; }
        public Match? NextMatch { get; set; }
        public MatchStatus Status { get; set; }
        public MatchResult? Result { get; set; }
    }
}