namespace GamebookApp.Backend.Models
{
    public class Player
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int HP { get; set; }
        public int CurrentRoomId { get; set; }
        public int BearHP { get; set; }
    }
}