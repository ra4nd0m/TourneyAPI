using TourneyAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace TourneyAPI.Data
{
    public class TournamentContext : IdentityDbContext<User>
    {
        public TournamentContext(DbContextOptions<TournamentContext> options) : base(options)
        {
        }
        public DbSet<Tournament> Tournaments { get; set; } = null!;
        public DbSet<Team> Teams { get; set; } = null!;
        public DbSet<Match> Matches { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Match>()
                .HasOne(m => m.NextMatch)
                .WithMany()
                .HasForeignKey(m => m.NextMatchId);

            modelBuilder.Entity<Match>()
                .HasOne(m => m.Team1)
                .WithMany()
                .HasForeignKey(m => m.Team1Id);

            modelBuilder.Entity<Match>()
                .HasOne(m => m.Team2)
                .WithMany()
                .HasForeignKey(m => m.Team2Id);

            modelBuilder.Entity<Match>()
                .HasOne(m => m.Result)
                .WithOne(mr => mr.Match)
                .HasForeignKey<MatchResult>(mr => mr.MatchId);

            modelBuilder.Entity<TournamentTeam>()
                .HasKey(tt => new { tt.TournamentId, tt.TeamId });

            modelBuilder.Entity<Tournament>()
                .HasOne(t => t.Admin)
                .WithMany(u => u.AdminTournaments)
                .HasForeignKey(t => t.AdminId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}