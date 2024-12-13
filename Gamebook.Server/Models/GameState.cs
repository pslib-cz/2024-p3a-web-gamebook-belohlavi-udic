namespace Gamebook.Server.Models
{
    public class GameState
    {
        public int ID { get; set; }
        public int PlayerID { get; set; }
        public required DateTime Timestamp { get; set; }
        public required string Data { get; set; }

        public virtual Player Player { get; set; } = null!;
    }
}