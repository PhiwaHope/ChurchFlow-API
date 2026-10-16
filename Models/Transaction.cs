using System.ComponentModel.DataAnnotations;

namespace ChurchFlow.api.Models
{
    public enum TransactionType
    {
        Income,
        Expense
    }

    public enum PaymentMethod
    {
        Cash,
        EFT,
        Card
        // add more here if needed, e.g. Card -> Debit/Credit, or Other
    }

    // Which table RefId refers to — e.g. Contribution, Sale, FundRaising.
    // We look this up in the controller to decide which lookup function
    // to call. "Other" covers plain expenses that don't link back to
    // any entity (e.g. "Rehearsal venue" in the screenshot).
    public enum RefType
    {
        Contribution,
        Sale,
        FundRaising,
        Event,
        Offering,
        Other
    }

    public class Transaction
    {
        [Key]
        public int TransId { get; set; }

        // What this transaction is FOR, in plain words — shown directly
        // in the UI (e.g. "Member Contribution - MUZ Closing Function")
        public string Description { get; set; } = string.Empty;

        // Whether this transaction adds money in or takes money out —
        // drives the +/- display and the Total Income/Expenses totals
        public TransactionType Type { get; set; }

        public decimal Amount { get; set; }

        public DateTime Date { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        // Generic pointer to "the thing this transaction is about" —
        // e.g. a ContrId if RefType is Contribution, a SaleId if
        // RefType is Sale, etc. Nullable because not every transaction
        // links back to one of these (e.g. a plain expense).
        // Not a real foreign key — the app decides which table to look
        // in based on RefType.
        public int? RefId { get; set; }

        // Nullable for the same reason as RefId above — a plain
        // expense transaction has no reference at all.
        public RefType? RefType { get; set; }
    }
}