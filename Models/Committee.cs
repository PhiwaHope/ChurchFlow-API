using System.ComponentModel.DataAnnotations;

namespace ChurchFlow.api.Models
{
    public class Committee
    {
        [Key]
        public int ComId { get; set; }
        public int MemberId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        //Navigation property
        public Member? Member { get; set; }
    }
}