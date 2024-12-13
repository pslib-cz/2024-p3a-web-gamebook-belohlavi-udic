using Gamebook.Server.Models;

namespace Gamebook.Server.ViewModels
{
    public class FileListVM
    {
        public Guid FileId { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public string ContentType { get; set; }
        public DateTime CreatedAt { get; set; }
        public User? CreatedBy { get; set; }
        public string CreatedById { get; set; }
    }
}