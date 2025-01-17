using System.ComponentModel.DataAnnotations;

namespace Gamebook.Server.Models
{
    public class Challenge
    {
        [Key]
        public int ID { get; set; }

        [StringLength(100)]
        public string Type { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [StringLength(1000)]
        public string SuccessOutcome { get; set; } = string.Empty;

        [StringLength(1000)]
        public string FailureOutcome { get; set; } = string.Empty;
    }
}
w