namespace MultiVendor.Domain.Entities;

public class OrderStatusHistory : BaseEntity
{
    public Guid OrderId { get; set; }
    public OrderStatus Status { get; set; }
    public string ChangedByUserId { get; set; } = string.Empty;
    public string? Notes { get; set; }

    public Order Order { get; set; } = null!;
}
