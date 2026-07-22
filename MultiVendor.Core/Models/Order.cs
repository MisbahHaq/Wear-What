namespace MultiVendor.Core.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public ApplicationUser? Customer { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Placed";
        public DateTime? DeliveredAt { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public string DeliveryMethod { get; set; } = "Standard";
        public decimal DeliveryFee { get; set; }
        public string PaymentMethod { get; set; } = "CashOnDelivery";
        public decimal TotalAmount { get; set; }
        public List<OrderItem> OrderItems { get; set; } = new();
    }
}

