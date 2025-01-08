namespace Gamebook.Server.Models
{
    public class Action
    {
        public Guid Id { get; set; } // Změna na Id
        public string ActionType { get; set; }
        public Guid RoomId { get; set; } // Změna na RoomId
        public virtual Room TargetRoom { get; set; }
    }
}