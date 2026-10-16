using System.ComponentModel.DataAnnotations;

namespace ChurchFlow.api.Models
{
    public class Contribution
    {
        [Key]
        public int ContrId { get; set; }
        //foreign key
        public int EventId { get; set; }
        public int MemberId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime LastPaidDate { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal TotalAmount { get; set; }
        //Navigation Properties for foreign keys
        public Event? Event { get; set; }
        public Member? Member { get; set; }
    }
}