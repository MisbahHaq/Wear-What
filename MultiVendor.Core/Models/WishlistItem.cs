namespace MultiVendor.Core.Models
{
    public class WishlistItem
    {
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
    }
}
