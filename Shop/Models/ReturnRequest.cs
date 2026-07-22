namespace Shop.Models
{
    public class ReturnRequest
    {
        public int Id { get; set; }
        public int OrderItemId { get; set; }
        public OrderItem? OrderItem { get; set; }
        public string BuyerId { get; set; } = string.Empty;
        public ApplicationUser? Buyer { get; set; }
        public string? SellerId { get; set; }
        public ApplicationUser? Seller { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? SellerResponse { get; set; }
        public string Status { get; set; } = "Pending";
        public string RefundStatus { get; set; } = "Pending";
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RespondedAt { get; set; }
        public DateTime? RefundedAt { get; set; }
    }
}
