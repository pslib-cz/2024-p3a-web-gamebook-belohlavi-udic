namespace Gamebook.Server.Models
{
    public class Connection
    {
        public int ID { get; set; }
        public int RoomID1 { get; set; }
        public int RoomID2 { get; set; }
        public required string ConnectionType { get; set; }

        // Navigation properties
        public virtual Room Room1 { get; set; } = null!;
        public virtual Room Room2 { get; set; } = null!;
    }
}
