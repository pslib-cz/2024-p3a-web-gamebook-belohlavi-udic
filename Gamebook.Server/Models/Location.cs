namespace Gamebook.Server.Models
{
    public class Location
    {
        public int LocationId { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
    }
}
