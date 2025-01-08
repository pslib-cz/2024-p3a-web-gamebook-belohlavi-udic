namespace Gamebook.Server.DTOs
{
    public class RoomDTO
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Exits { get; set; }
        public List<ConnectionDTO> OutgoingConnections { get; set; }
        public List<ConnectionDTO> IncomingConnections { get; set; }
    }
}