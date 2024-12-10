using Gamebook.Server.Models;

namespace Gamebook.Server.ViewModels
{
    public class UserListVM
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public ICollection<Role>? Roles { get; set; }
    }
}
