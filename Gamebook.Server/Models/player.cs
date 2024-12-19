using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Gamebook.Server.Models
{
    public class Player
    {
        public int ID { get; set; }
        public string UserId { get; set; } = string.Empty;  // ID uživatele z ASP.NET Identity
        public int CurrentRoomID { get; set; }
        public int HP { get; set; }
        public string? Status { get; set; }

        public virtual Room CurrentRoom { get; set; } = null!;
        public virtual ICollection<GameState> GameStates { get; set; } = new List<GameState>();
        [JsonIgnore]
        public virtual User User { get; set; } = null!;
    }
}