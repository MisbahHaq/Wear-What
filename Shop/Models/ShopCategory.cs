namespace Shop.Models
{
    public class ShopCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ShopId { get; set; }
        public ShopItem? Shop { get; set; }
        public List<Product> Products { get; set; } = new();
    }
}
