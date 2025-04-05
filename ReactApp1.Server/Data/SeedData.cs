using GamebookApp.Backend.Models;

using GamebookApp.Backend.Models;
using System.Linq;

namespace GamebookApp.Backend.Data
{
    public static class SeedData
    {
        public static void Initialize(AppDbContext context)
        {
            if (context.Rooms.Any())
            {
                return; // Databáze již obsahuje data
            }

            var rooms = new Room[]
            {
                new Room
                {
                    Id = 1,
                    Name = "Vstup do džungle",
                    Description = "Stojíš na okraji husté džungle. Vzduch je vlhký a slyšíš zvuky divokých zvířat. Před tebou je několik možných cest.",
                    ImagePath = "/images/startovnitabor.webp"
                },
                new Room
                {
                    Id = 2,
                    Name = "Hustá džungle",
                    Description = "Vegetace je zde tak hustá, že téměř nevidíš na krok. Zvuky džungle jsou všude kolem tebe.",
                    ImagePath = "/images/bujnadzungle.webp"
                },
                new Room
                {
                    Id = 3,
                    Name = "Zamaskovaná past",
                    Description = "Před sebou vidíš něco, co vypadá jako past. Na zemi jsou podezřelé znamení.",
                    ImagePath = "/images/past.webp"
                },
                new Room
                {
                    Id = 4,
                    Name = "Nebezpečný most",
                    Description = "Před tebou je starý, polorozpadlý most přes hlubokou propast. Vypadá velmi nestabilně.",
                    ImagePath = "/images/most.webp"
                },
                new Room
                {
                    Id = 5,
                    Name = "Jeskyně s medvědem",
                    Description = "Vstoupil jsi do temné jeskyně. Najednou slyšíš hrozivý řev. Obrovský medvěd se na tebe řítí!",
                    ImagePath = "/images/medved.webp"
                },
                new Room
                {
                    Id = 6,
                    Name = "Vodopád",
                    Description = "Stojíš u krásného vodopádu. Voda se třpytí v paprscích slunce. Je to skvělé místo k odpočinku.",
                    ImagePath = "/images/vodopad.webp"
                },
                new Room
                {
                    Id = 7,
                    Name = "Bezpečná zóna",
                    Description = "Konečně jsi našel cestu ven z džungle! Jsi v bezpečí a můžeš si oddechnout.",
                    ImagePath = "/images/bezpecna.webp"
                },
                new Room
                {
                    Id = 8,
                    Name = "Řeka",
                    Description = "Stojíš u divoké řeky. Proud je silný a voda vypadá nebezpečně.",
                    ImagePath = "/images/reka.webp"
                },
                new Room
                {
                    Id = 9,
                    Name = "Konec hry",
                    Description = "Bohužel jsi nepřežil nebezpečí džungle. Tvé dobrodružství zde končí.",
                    ImagePath = "/images/starmost.webp"
                }
            };

            context.Rooms.AddRange(rooms);
            context.SaveChanges();

            var exits = new Exit[]
            {
                // Z místnosti 1
                new Exit { RoomId = 1, Direction = "Jít na východ", TargetRoomId = 2 },
                new Exit { RoomId = 1, Direction = "Jít na západ", TargetRoomId = 8 },
                
                // Z místnosti 2
                new Exit { RoomId = 2, Direction = "Pokračovat vpřed", TargetRoomId = 3 },
                new Exit { RoomId = 2, Direction = "Vrátit se", TargetRoomId = 1 },
                
                // Z místnosti 3
                new Exit { RoomId = 3, Direction = "Přeskočit past", TargetRoomId = 4 },
                new Exit { RoomId = 3, Direction = "Obejít past", TargetRoomId = 5 },
                
                // Z místnosti 4
                new Exit { RoomId = 4, Direction = "Přejít most rychle", TargetRoomId = 6 },
                new Exit { RoomId = 4, Direction = "Přejít most opatrně", TargetRoomId = 9 },
                
                // Z místnosti 6
                new Exit { RoomId = 6, Direction = "Pokračovat dál", TargetRoomId = 7 },
                
                // Z místnosti 8
                new Exit { RoomId = 8, Direction = "Přebrodit řeku", TargetRoomId = 9 },
                new Exit { RoomId = 8, Direction = "Hledat jinou cestu", TargetRoomId = 2 }
            };

            context.Exits.AddRange(exits);
            context.SaveChanges();
        }
    }
}