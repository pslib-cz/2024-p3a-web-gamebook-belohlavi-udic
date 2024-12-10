namespace Gamebook.Server.Models
{
    public class Action
    {
        public Guid ActionId { get; set; }
        public string ActionType { get; set; }
        public Guid TargetRoomId { get; set; }
        public virtual Room TargetRoom { get; set; }
    }

}
