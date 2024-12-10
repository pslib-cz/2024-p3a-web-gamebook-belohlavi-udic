using Gamebook.Server.Constants;
using Gamebook.Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Gamebook.Server.Data
{
    public class GamebookDbContext : IdentityDbContext<User, Models.Role, string>
    {
        public DbSet<Models.File> Files { get; set; } = null!;
        public override DbSet<User> Users { get; set; } = null!;
        public DbSet<Room> Rooms { get; set; } = null!;
        public DbSet<GameState> GameStates { get; set; } = null!;
        public DbSet<Models.Action> Actions { get; set; } = null!;

        public GamebookDbContext(DbContextOptions<GamebookDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Existing roles and users seeding logic
            var adminRoleId = Guid.NewGuid().ToString();
            var authorRoleId = Guid.NewGuid().ToString();
            modelBuilder.Entity<Models.Role>(options =>
            {
                options.HasData(
                    new Models.Role
                    {
                        Id = adminRoleId,
                        Name = Constants.Role.Admin,
                        NormalizedName = Constants.Role.Admin.ToUpper()
                    },
                    new Models.Role
                    {
                        Id = authorRoleId,
                        Name = Constants.Role.Author,
                        NormalizedName = Constants.Role.Author.ToUpper()
                    }
                );
            });
            var mainAdminId = Guid.NewGuid().ToString();
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasMany(u => u.Roles).WithMany(r => r.Users).UsingEntity<IdentityUserRole<string>>();
                entity.HasData(
                    new User
                    {
                        Id = mainAdminId,
                        UserName = "admin@localhost.test",
                        NormalizedUserName = "ADMIN@LOCALHOST.TEST",
                        Email = "admin@localhost.test",
                        NormalizedEmail = "ADMIN@LOCALHOST.TEST",
                        EmailConfirmed = true,
                        PasswordHash = new PasswordHasher<User>().HashPassword(null!, "admin1234"),
                        SecurityStamp = string.Empty
                    }
                );
            });
            modelBuilder.Entity<IdentityUserRole<string>>(entity =>
            {
                entity.HasKey(x => new { x.RoleId, x.UserId });
                entity.HasData(
                    new IdentityUserRole<string>
                    {
                        RoleId = adminRoleId,
                        UserId = mainAdminId
                    },
                    new IdentityUserRole<string>
                    {
                        RoleId = authorRoleId,
                        UserId = mainAdminId
                    }
                );
            });

            // New tables for game mechanics
            modelBuilder.Entity<Room>(entity =>
            {
                entity.HasKey(r => r.RoomId);
                entity.Property(r => r.Name).IsRequired();
                entity.HasMany(r => r.Actions)
                      .WithOne(a => a.TargetRoom)
                      .HasForeignKey(a => a.TargetRoomId);
            });

            modelBuilder.Entity<GameState>(entity =>
            {
                entity.HasKey(gs => gs.GameStateId);
                entity.HasOne(gs => gs.CurrentRoom)
                      .WithMany()
                      .HasForeignKey(gs => gs.CurrentRoomId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Models.Action>(entity =>
            {
                entity.HasKey(a => a.ActionId);
                entity.Property(a => a.ActionType).IsRequired();
                entity.HasOne(a => a.TargetRoom)
                      .WithMany()
                      .HasForeignKey(a => a.TargetRoomId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
