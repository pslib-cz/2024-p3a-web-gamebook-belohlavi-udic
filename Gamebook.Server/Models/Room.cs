namespace Gamebook.Server.Models
{
    public class Room
    {
        public Guid RoomId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ICollection<Action> Actions { get; set; }
    }

}
