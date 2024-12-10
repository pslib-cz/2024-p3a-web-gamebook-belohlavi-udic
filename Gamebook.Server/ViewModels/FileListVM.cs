using Gamebook.Server.Models;

namespace Gamebook.Server.ViewModels
{
    public class FileListVM
    {
        public int FileId { get; set; }
        public required string Name { get; set; }
        public required long Size { get; set; }
        public required string ContentType { get; set; }
        public required DateTime CreatedAt { get; set; } = DateTime.Now;
        public User? CreatedBy { get; set; }
        public required string CreatedById { get; set; }
    }
}
