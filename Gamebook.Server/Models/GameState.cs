namespace Gamebook.Server.Models
{
    public class GameState
    {
        public Guid GameStateId { get; set; }
        public Guid UserId { get; set; }
        public Guid CurrentRoomId { get; set; }
        public int HP { get; set; }
        public virtual Room CurrentRoom { get; set; }
    }

}
