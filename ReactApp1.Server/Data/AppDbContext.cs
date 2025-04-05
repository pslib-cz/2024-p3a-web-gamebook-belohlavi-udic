using GamebookApp.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace GamebookApp.Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Room> Rooms { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Exit> Exits { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Room>()
                .HasMany(r => r.Exits)
                .WithOne(e => e.Room)
                .HasForeignKey(e => e.RoomId);
        }
    }
}