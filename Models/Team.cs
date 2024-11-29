namespace TourneyAPI.Models{
    public class Team{
        public int Id { get; set; }
        public required string Name { get; set; }
        public List<Tournament>? Tournaments { get; set; }
    }
}