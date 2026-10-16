namespace ChurchFlow.api.Models
{

    public class PaymentRequest
    {
        public int EventId { get; set; }
        public int MemberId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    }
}