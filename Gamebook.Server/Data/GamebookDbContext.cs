using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Gamebook.Server.Models;

namespace Gamebook.Server.Data
{
    public class GamebookDbContext : IdentityDbContext<User, Role, string>
    {
        public GamebookDbContext(DbContextOptions<GamebookDbContext> options) : base(options)
        {
        }

        public DbSet<Room> Rooms { get; set; } = null!;
        public DbSet<Connection> Connections { get; set; } = null!;
        public DbSet<Challenge> Challenges { get; set; } = null!;
        public DbSet<Player> Players { get; set; } = null!;
        public DbSet<GameState> GameStates { get; set; } = null!;
        public DbSet<Models.File> Files { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Room configurations
            modelBuilder.Entity<Room>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.Description).IsRequired();
            });

            // Connection configurations
            modelBuilder.Entity<Connection>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.ConnectionType).IsRequired();

                entity.HasOne(d => d.Room1)
                    .WithMany(p => p.OutgoingConnections)
                    .HasForeignKey(d => d.RoomID1)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Room2)
                    .WithMany(p => p.IncomingConnections)
                    .HasForeignKey(d => d.RoomID2)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Challenge configurations
            modelBuilder.Entity<Challenge>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Type).IsRequired();
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.SuccessOutcome).IsRequired();
                entity.Property(e => e.FailureOutcome).IsRequired();
            });

            // Player configurations
            modelBuilder.Entity<Player>(entity =>
            {
                entity.HasKey(e => e.ID);

                entity.HasOne(d => d.CurrentRoom)
                    .WithMany(p => p.Players)
                    .HasForeignKey(d => d.CurrentRoomID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // GameState configurations
            modelBuilder.Entity<GameState>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Timestamp).IsRequired();
                entity.Property(e => e.Data).IsRequired();

                entity.HasOne(d => d.Player)
                    .WithMany(p => p.GameStates)
                    .HasForeignKey(d => d.PlayerID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Zachování existující konfigurace pro Identity
            var adminRoleId = Guid.NewGuid().ToString();
            var authorRoleId = Guid.NewGuid().ToString();
            // ... (zbytek vaší existující konfigurace pro role a uživatele)
        }
    }
}