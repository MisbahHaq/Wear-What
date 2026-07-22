namespace Shop.Models
{
    public class ShopItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Approved";
        public bool IsSuspended { get; set; }
        public string ProfileBannerUrl { get; set; } = string.Empty;
        public string ProfileImageUrl { get; set; } = string.Empty;
        public string OwnerId { get; set; } = string.Empty;
        public ApplicationUser? Owner { get; set; }
        public List<Product> Products { get; set; } = new();
    }
}