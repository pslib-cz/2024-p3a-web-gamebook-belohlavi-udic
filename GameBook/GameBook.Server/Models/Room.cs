using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gamebook.Server.Models
{
    public class Room
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [StringLength(500)]
        public string Exits { get; set; } = string.Empty;

        public virtual ICollection<Connection>? ConnectionsFrom { get; set; }
        public virtual ICollection<Connection>? ConnectionsTo { get; set; }
        public virtual ICollection<Challenge>? Challenges { get; set; }
    }
}