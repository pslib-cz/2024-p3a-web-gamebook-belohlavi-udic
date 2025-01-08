using Gamebook.Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Gamebook.Server.Data
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new GamebookDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<GamebookDbContext>>()))
            {
                // Check if roles exist, and create them if not
                var roleManager = serviceProvider.GetRequiredService<RoleManager<Role>>();
                EnsureRoleExists(roleManager, "Admin");
                EnsureRoleExists(roleManager, "Author");

                // Check if the admin user exists, and create it if not
                var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
                var adminUser = userManager.FindByNameAsync("admin").Result;
                if (adminUser == null)
                {
                    adminUser = new User
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
                    var result = userManager.CreateAsync(adminUser, "Admin123!").Result;
                    if (result.Succeeded)
                    {
                        userManager.AddToRoleAsync(adminUser, "Admin").Wait();
                    }
                }

                // Look for any rooms. If there are any, the DB has been seeded.
                if (context.Rooms.Any())
                {
                    return; // DB has been seeded
                }

                // Seed Rooms
                context.Rooms.AddRange(
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
                context.SaveChanges();

                // Seed Connections
                context.Connections.AddRange(
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
                context.SaveChanges();

                // Seed Challenges
                context.Challenges.AddRange(
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
                context.SaveChanges();
            }
        }
        private static void EnsureRoleExists(RoleManager<Role> roleManager, string roleName)
        {
            if (!roleManager.RoleExistsAsync(roleName).Result)
            {
                var role = new Role { Name = roleName, NormalizedName = roleName.ToUpper() };
                var result = roleManager.CreateAsync(role).Result;
                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to create role '{roleName}'");
                }
            }
        }
    }
}