namespace Gamebook.Server.DTOs
{
    public class ConnectionDTO
    {
        public int ID { get; set; }
        public int RoomID2 { get; set; } // Only include the related room ID
        public string ConnectionType { get; set; }
    }
}