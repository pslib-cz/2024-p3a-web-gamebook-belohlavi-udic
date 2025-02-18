using GamebookApp.Backend.Models;
using System.Linq;

namespace GamebookApp.Backend.Data
{
    public static class SeedData
    {
        public static void Initialize(AppDbContext context)
        {
            context.Database.EnsureCreated();

            if (!context.Rooms.Any())
            {
                //rooms
                var startRoom = new Room
                {
                    Name = "Startovní tábor",
                    Description = "Nacházíte se ve startovním táboře. Kudy se vydáte?",
                    ImagePath = "/images/startovnitabor.webp"
                };

                var forestRoom = new Room
                {
                    Name = "Lesní cesta",
                    Description = "Dorazil jsi na lesní cestu. Vidíš před sebou pasti, jak se zachováš?",
                    ImagePath = "/images/lesnicesta.webp"
                };

                var swampRoom = new Room
                {
                    Name = "Bažina",
                    Description = "Dorazil jsi do bažiny. Slyšíš zvuky nebezpečí.",
                    ImagePath = "/images/bazina.webp"
                };

                var bridgeRoom = new Room
                {
                    Name = "Starý most",
                    Description = "Před tebou je starý most. Jak ho překonáš?",
                    ImagePath = "/images/starymost.webp"
                };

                var bearRoom = new Room
                {
                    Name = "Jeskyně s medvědem",
                    Description = "Dostal jsi se do jeskyně, kde spí medvěd. Musíš se s ním vypořádat!",
                    ImagePath = "/images/medved.webp"
                };

                var riverRoom = new Room
                {
                    Name = "Řeka",
                    Description = "Dorazil jsi k divoké řece. Jak ji překonáš?",
                    ImagePath = "/images/reka.webp"
                };

                var waterfallRoom = new Room
                {
                    Name = "Vodopád",
                    Description = "Před tebou je vodopád, za kterým vidíš zvláštní světlo.",
                    ImagePath = "/images/vodopad.webp"
                };

                var safeZoneRoom = new Room
                {
                    Name = "Bezpečná zóna",
                    Description = "Konečně jsi v bezpečí! Gratulujeme k dokončení hry!",
                    ImagePath = "/images/SucessScreen.webp"
                };

                var deathRoom = new Room
                {
                    Name = "Konec hry",
                    Description = "ZEMŘEL JSI",
                    ImagePath = "/images/KaputScreen2.webp"
                };

                context.Rooms.AddRange(startRoom, forestRoom, swampRoom, bridgeRoom,
                                    bearRoom, riverRoom, waterfallRoom, safeZoneRoom, deathRoom);
                context.SaveChanges();

                
                var exits = new[]
                {
                    // Startovní tábor
                    new Exit { Room = startRoom, Direction = "Jít na východ", TargetRoomId = forestRoom.Id },
                    new Exit { Room = startRoom, Direction = "Jít na západ", TargetRoomId = swampRoom.Id },

                    // Lesní cesta
                    new Exit { Room = forestRoom, Direction = "Přeskočit past", TargetRoomId = bridgeRoom.Id },
                    new Exit { Room = forestRoom, Direction = "Nevšimnout si pasti", TargetRoomId = deathRoom.Id },
                    new Exit { Room = forestRoom, Direction = "Otočit se a jít zpět", TargetRoomId = startRoom.Id },

                    // Bažina
                    new Exit { Room = swampRoom, Direction = "Vyhnout se nebezpečí", TargetRoomId = bridgeRoom.Id },
                    new Exit { Room = swampRoom, Direction = "Ignorovat zvuky", TargetRoomId = deathRoom.Id },
                    new Exit { Room = swampRoom, Direction = "Otočit se a jít zpět", TargetRoomId = startRoom.Id },

                    // Most
                    new Exit { Room = bridgeRoom, Direction = "Přejít most rychle", TargetRoomId = deathRoom.Id },
                    new Exit { Room = bridgeRoom, Direction = "Přejít most s velkou opatrností", TargetRoomId = bearRoom.Id },
                    new Exit { Room = bridgeRoom, Direction = "Otočit se a jít zpět", TargetRoomId = startRoom.Id },

                    // Souboj s medvědem

                    // Řeka
                    new Exit { Room = riverRoom, Direction = "Použít loďku", TargetRoomId = deathRoom.Id },
                    new Exit { Room = riverRoom, Direction = "Přeplavat řeku", TargetRoomId = waterfallRoom.Id },
                    new Exit { Room = riverRoom, Direction = "Vrátit se", TargetRoomId = bridgeRoom.Id },

                    // Vodopád
                    new Exit { Room = waterfallRoom, Direction = "Prozkoumat světlo", TargetRoomId = safeZoneRoom.Id },
                    new Exit { Room = waterfallRoom, Direction = "Ignorovat světlo", TargetRoomId = deathRoom.Id },

                    // Safe Zone
                    new Exit { Room = safeZoneRoom, Direction = "Dokončit hru", TargetRoomId = safeZoneRoom.Id }
                };

                context.Exits.AddRange(exits);
                context.SaveChanges();
            }

            if (!context.Players.Any())
            {
                var player = new Player { HP = 100, CurrentRoomId = 1 };
                context.Players.Add(player);
                context.SaveChanges();
            }
        }
    }
}