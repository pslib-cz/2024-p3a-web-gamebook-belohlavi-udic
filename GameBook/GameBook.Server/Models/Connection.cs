using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gamebook.Server.Models
{
    public class Connection
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public int RoomID1 { get; set; }

        [Required]
        public int RoomID2 { get; set; }

        [StringLength(100)]
        public string ConnectionType { get; set; } = string.Empty;

        [ForeignKey("RoomID1")]
        public virtual Room? Room1 { get; set; }

        [ForeignKey("RoomID2")]
        public virtual Room? Room2 { get; set; }
    }
}