using System.Collections.Generic;

namespace GamebookApp.Backend.Models
{
    public class Room
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }
        public ICollection<Exit> Exits { get; set; } = new List<Exit>();
    }
}