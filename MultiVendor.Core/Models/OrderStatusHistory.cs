namespace MultiVendor.Core.Models
{
    public class OrderStatusHistory
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order? Order { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ChangedByUserId { get; set; }
        public ApplicationUser? ChangedByUser { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? Note { get; set; }
    }
}

