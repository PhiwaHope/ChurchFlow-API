namespace ChurchFlow.api.Models
{
    public class Event
    {
        public int EventId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal ContributionAmount { get; set; }
        public string Poster { get; set; } = string.Empty;
        public string InvitationLetter { get; set; } = string.Empty;
        //Navigation property
        public List<Contribution> Contributions { get; set; } = new();
    }
}