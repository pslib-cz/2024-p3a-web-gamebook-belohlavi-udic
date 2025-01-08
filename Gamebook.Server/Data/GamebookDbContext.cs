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
                entity.Property(e => e.RoomID1).IsRequired();
                entity.Property(e => e.RoomID2).IsRequired();

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
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.CurrentRoomID).IsRequired();

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
                entity.Property(e => e.PlayerID).IsRequired();

                entity.HasOne(d => d.Player)
                    .WithMany(p => p.GameStates)
                    .HasForeignKey(d => d.PlayerID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Konstantní GUIDy pro role
            const string ADMIN_ROLE_ID = "55555555-4444-3333-2222-111111111111";
            const string AUTHOR_ROLE_ID = "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee";

            // Vytvoření rolí
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = ADMIN_ROLE_ID, Name = "Admin", NormalizedName = "ADMIN" },
                new Role { Id = AUTHOR_ROLE_ID, Name = "Author", NormalizedName = "AUTHOR" }
            );

            // Vytvoření uživatele
            var adminUser = new User
            {
                Id = "99999999-8888-7777-6666-555555555555",
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                Email = "admin@example.com",
                NormalizedEmail = "ADMIN@EXAMPLE.COM",
                EmailConfirmed = true,
                LockoutEnabled = false,
                SecurityStamp = string.Empty
            };
            adminUser.PasswordHash = new PasswordHasher<User>().HashPassword(adminUser, "Admin123!");
            modelBuilder.Entity<User>().HasData(adminUser);

            // Přiřazení role uživateli
            modelBuilder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string> { UserId = adminUser.Id, RoleId = ADMIN_ROLE_ID }
            );

            // Vložení základních dat (Rooms, Connections, Challenges)
            modelBuilder.Entity<Room>().HasData(
                new Room
                {
                    ID = 1,
                    Name = "Mysterious Cave Entrance",
                    Description = "You stand at the mouth of a dark and mysterious cave. A chilling draft emanates from within, carrying the scent of damp earth and an unknown, ancient presence. The entrance is shrouded in shadows, making it difficult to discern what lies beyond. You can see a path leading north into the darkness.",
                    Exits = "north"
                },
                new Room
                {
                    ID = 2,
                    Name = "Dark Cavern",
                    Description = "You enter a dimly lit cavern. The air is heavy and still. Water drips from unseen stalactites, echoing in the silence. You can go back south",
                    Exits = "south"
                }
            );

            modelBuilder.Entity<Connection>().HasData(
                new Connection
                {
                    ID = 1,
                    RoomID1 = 1,
                    RoomID2 = 2,
                    ConnectionType = "north"
                },
                new Connection
                {
                    ID = 2,
                    RoomID1 = 2,
                    RoomID2 = 1,
                    ConnectionType = "south"
                }
            );

            modelBuilder.Entity<Challenge>().HasData(
                new Challenge
                {
                    ID = 1,
                    Type = "Hádanka",
                    Description = "Na zemi leží starý svitek s hádankou: 'Co má oči, ale nevidí?'",
                    SuccessOutcome = "Správně jsi odpověděl! Otevřela se tajná schránka a v ní jsi našel 10 zlaťáků.",
                    FailureOutcome = "Špatná odpověď. Zklamal jsi ducha jeskyně."
                },
                new Challenge
                {
                    ID = 2,
                    Type = "Souboj",
                    Description = "Cestu ti zatarasil divoký vlk. Musíš ho porazit v souboji!",
                    SuccessOutcome = "Vlk je poražen! Můžeš pokračovat v cestě.",
                    FailureOutcome = "Vlk tě přemohl a ukousl ti kus zdraví!"
                }
            );
        }
    }
}