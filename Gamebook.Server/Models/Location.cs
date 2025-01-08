namespace Gamebook.Server.Models
{
    public class Location
    {
        public int Id { get; set; } // Změna na Id
        public required string Title { get; set; }
        public required string Description { get; set; }
    }
}