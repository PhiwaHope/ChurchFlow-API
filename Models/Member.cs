namespace ChurchFlow.api.Models
{
    public class Member
    {
        public int MemberId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string FundingStatus { get; set; } = string.Empty;
        public DateTime JoinDate { get; set; }
        //Navigation property
        public List<Contribution> Contributions { get; set; } = new();
    }
}