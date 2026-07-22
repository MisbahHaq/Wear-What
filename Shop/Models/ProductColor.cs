namespace Shop.Models
{
    public class ProductColor
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public string ColorName { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
    }
}
