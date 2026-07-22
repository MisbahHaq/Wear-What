namespace MultiVendor.Core.Models
{
    public class ShopFollow
    {
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
        public int ShopId { get; set; }
        public ShopItem? Shop { get; set; }
    }
}
