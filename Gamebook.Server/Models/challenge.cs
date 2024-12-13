namespace Gamebook.Server.Models
{
    public class Challenge
    {
        public int ID { get; set; }
        public required string Type { get; set; }
        public required string Description { get; set; }
        public required string SuccessOutcome { get; set; }
        public required string FailureOutcome { get; set; }
    }
}
