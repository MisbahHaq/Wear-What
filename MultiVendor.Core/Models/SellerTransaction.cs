namespace MultiVendor.Core.Models
{
    public class SellerTransaction
    {
        public int Id { get; set; }
        public string SellerId { get; set; } = string.Empty;
        public ApplicationUser? Seller { get; set; }
        public int OrderId { get; set; }
        public Order? Order { get; set; }
        public decimal CommissionRate { get; set; }
        public decimal CommissionAmount { get; set; }
        public decimal NetAmount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

