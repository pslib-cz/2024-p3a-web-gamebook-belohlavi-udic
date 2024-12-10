namespace Gamebook.Server.ViewModels
{
    public class LocationIM
    {
        public int LocationId { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
    }
}
