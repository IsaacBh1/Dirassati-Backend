using Dirassati_Backend.Data.Enums;

namespace Dirassati_Backend.Data.Models;


public class StudentPayment
{

    public Guid BillId { get; set; }

    public Guid StudentId { get; set; }
    public Guid ParentId { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public decimal AmountPaid { get; set; } // The actual amount confirmed by gateway

    public string? PaymentGatewayCheckoutId { get; set; } // Chargily's Checkout ID

    public string? PaymentGatewayTransactionId { get; set; } // Chargily's final transaction/invoice ID (optional)

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


    // Navigation Properties
    public virtual Bill Bill { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
    public virtual Parent Parent { get; set; } = null!;


}
