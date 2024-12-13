using MiNET;
using System.Numerics;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Gamebook.Server.Models
{
    public class Room
    {
        public int ID { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public string? Exits { get; set; }

        public virtual ICollection<Connection> OutgoingConnections { get; set; } = new List<Connection>();
        public virtual ICollection<Connection> IncomingConnections { get; set; } = new List<Connection>();
        public virtual ICollection<Player> Players { get; set; } = new List<Player>();
    }
}