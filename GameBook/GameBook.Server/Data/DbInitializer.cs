// V GameBook.Server/Data/DbInitializer.cs
using GameBook.Server.Data;
using GameBook.Server.Models;

public static class DbInitializer
{
    public static void Initialize(GameBookContext context)
    {
        context.Database.EnsureCreated();

        // Zkontrolujeme, jestli už existují nějaké místnosti
        if (context.Rooms.Any())
        {
            return;   // DB už byla inicializována
        }

        var rooms = new Room[]
        {
            new Room{ID=1, Name="Startovní tábor", Description="Nacházíte se v opuštěném táboře uprostřed džungle. Všude kolem je hustá vegetace.", Exits="východ, západ"},
            new Room{ID=2, Name="Lesní cesta", Description="Úzká pěšina vede skrz hustou džungli. Slyšíte zvuky divokých zvířat.", Exits="sever, jih"},
            new Room{ID=3, Name="Starý most", Description="Před vámi se tyčí starý provazový most. Vypadá nebezpečně.", Exits="východ, západ"}
        };

        context.Rooms.AddRange(rooms);
        context.SaveChanges();

        var connections = new Connection[]
        {
            new Connection{ID=1, RoomID1=1, RoomID2=2, ConnectionType="Jít na východ"},
            new Connection{ID=2, RoomID1=2, RoomID2=3, ConnectionType="Jít na sever"}
        };

        context.Connections.AddRange(connections);
        context.SaveChanges();
    }
}