using Gamebook.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace Gamebook.Server.Data
{
    public class GamebookContext : DbContext
    {
        public GamebookContext(DbContextOptions<GamebookContext> options)
            : base(options)
        {
        }

        public DbSet<Room> Rooms { get; set; }
        public DbSet<Connection> Connections { get; set; }
        public DbSet<Challenge> Challenges { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<GameState> GameStates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Connection>()
                .HasOne(c => c.Room1)
                .WithMany(r => r.ConnectionsFrom)
                .HasForeignKey(c => c.RoomID1)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Connection>()
                .HasOne(c => c.Room2)
                .WithMany(r => r.ConnectionsTo)
                .HasForeignKey(c => c.RoomID2)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Player>()
                .HasOne(p => p.CurrentRoom)
                .WithMany()
                .HasForeignKey(p => p.CurrentRoomID)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}