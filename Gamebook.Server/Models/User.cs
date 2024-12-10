using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Gamebook.Server.Models
{
    [Table("AspNetUsers")]
    public class User : IdentityUser<string>
    {
        public override string Id { get; set; } = Guid.NewGuid().ToString();
        [JsonIgnore]
        public ICollection<Role>? Roles { get; set; }
    }
}
