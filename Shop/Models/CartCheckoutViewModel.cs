namespace Shop.Models
{
    public class CartCheckoutItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? ShopName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }

        public decimal Total => UnitPrice * Quantity;
    }

    public class CartCheckoutViewModel
    {
        public List<CartCheckoutItemViewModel> Items { get; set; } = new();
        public decimal ProductsSubtotal => Items.Sum(i => i.Total);

        public const decimal ExpressDeliveryFee = 150m;

        public string DeliveryMethod { get; set; } = "Standard";
        public decimal DeliveryFee => DeliveryMethod == "Express" ? ExpressDeliveryFee : 0m;

        public decimal Total => ProductsSubtotal + DeliveryFee;

        public string CustomerFullName { get; set; } = string.Empty;
        public string CustomerContactNumber { get; set; } = string.Empty;

        public string ShippingAddress { get; set; } = string.Empty;

        public string PaymentMethod { get; set; } = "CashOnDelivery";
    }
}
