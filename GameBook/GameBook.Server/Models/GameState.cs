using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gamebook.Server.Models
{
    public class GameState
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public int PlayerID { get; set; }

        public DateTime Timestamp { get; set; }

        [StringLength(4000)]
        public string Data { get; set; } = string.Empty;

        [ForeignKey("PlayerID")]
        public virtual Player? Player { get; set; }
    }
}