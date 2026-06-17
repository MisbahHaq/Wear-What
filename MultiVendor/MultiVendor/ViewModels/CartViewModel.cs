using MultiVendor.DTOs;

namespace MultiVendor.ViewModels;

public sealed class CartViewModel
{
    public List<CartItemResponseDto> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal Shipping { get; set; }
    public decimal GrandTotal => Subtotal + Shipping;
    public int ItemCount => Items.Sum(i => i.Quantity);
}

public sealed class CartUpdateDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
