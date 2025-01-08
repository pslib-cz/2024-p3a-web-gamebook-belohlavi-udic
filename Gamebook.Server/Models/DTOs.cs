public class RoomDTO
{
    public int ID { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Exits { get; set; }
    public List<ConnectionDTO> OutgoingConnections { get; set; }
    public List<ConnectionDTO> IncomingConnections { get; set; }
}

public class ConnectionDTO
{
    public int ID { get; set; }
    public int RoomID2 { get; set; } // Only include the related room ID
    public string ConnectionType { get; set; }
}