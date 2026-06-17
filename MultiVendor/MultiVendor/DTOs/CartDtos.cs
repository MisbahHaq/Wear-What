using System.ComponentModel.DataAnnotations;

namespace MultiVendor.DTOs;

public class CartItemDto
{
    [Required]
    public int ProductId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}

public class CheckoutDto
{
    [Required]
    public List<CartItemDto> Items { get; set; } = new();

    [Required]
    [MaxLength(500)]
    public string ShippingAddress { get; set; } = string.Empty;
}

public class CartItemResponseDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ShopName { get; set; } = string.Empty;
    public int ShopId { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
    public int AvailableStock { get; set; }
    public string? CoverImageUrl { get; set; }
}
