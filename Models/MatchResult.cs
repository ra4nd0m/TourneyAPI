namespace TourneyAPI.Models
{
   
    public class MatchResult
    {
        public int Id { get; set; }
        public int MatchId { get; set; }
        public int Team1Score { get; set; }
        public int Team2Score { get; set; }
        public int? WinnerId { get; set; }
        public Match? Match { get; set; }
    }
}