using System.Text.Json.Serialization;

namespace GamebookApp.Backend.Models
{
    public class Exit
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        [JsonIgnore]
        public Room Room { get; set; }
        public string Direction { get; set; }
        public int TargetRoomId { get; set; }
    }
}