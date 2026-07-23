using MultiVendor.Domain.Entities;

namespace MultiVendor.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string PaymentMethod { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public string? PaymentGateway { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateTime? PaidAt { get; set; }
    public string? FailureReason { get; set; }
    public string? MetadataJson { get; set; }

    public Order Order { get; set; } = null!;
}
