using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gamebook.Server.Models
{
    public class Player
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public int CurrentRoomID { get; set; }

        public int HP { get; set; }

        [StringLength(100)]
        public string Status { get; set; } = string.Empty;

        [ForeignKey("CurrentRoomID")]
        public virtual Room? CurrentRoom { get; set; }

        public virtual ICollection<GameState>? GameStates { get; set; }
    }
}