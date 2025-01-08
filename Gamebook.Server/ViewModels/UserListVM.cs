namespace Gamebook.Server.ViewModels
{
    public class UserListVM
    {
        public required string Id { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public IList<string> Roles { get; set; } // Change the type to IList<string>
    }
}