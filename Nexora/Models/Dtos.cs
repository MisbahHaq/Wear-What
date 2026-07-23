namespace Nexora.Models;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? Gender { get; set; }
    public string? ImageUrl { get; set; }
    public string? ImageUrls { get; set; }
    public string? Tags { get; set; }
    public string? Colors { get; set; }
}

public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? Gender { get; set; }
    public string? ImageUrl { get; set; }
    public string? ImageUrls { get; set; }
    public string? Tags { get; set; }
    public string? Colors { get; set; }
}

public class UpdateProductDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public int? Stock { get; set; }
    public string? Gender { get; set; }
    public string? ImageUrl { get; set; }
    public string? ImageUrls { get; set; }
    public string? Tags { get; set; }
    public string? Colors { get; set; }
}

public class OrderDto
{
    public int Id { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? DeliveryMethod { get; set; }
    public string? PaymentMethod { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public string? Size { get; set; }
    public string? Color { get; set; }
}

public class CheckoutDto
{
    public string Address { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? DeliveryMethod { get; set; }
    public string? PaymentMethod { get; set; }
}

public class UpdateOrderStatusDto
{
    public string Status { get; set; } = string.Empty;
}

public class UserDto
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
}

public class LoginRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
}

public class RegisterRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Address { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class UpdateProfileDto
{
    public string? Name { get; set; }
    public string? Address { get; set; }
}

public class CartDto
{
    public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
    public decimal TotalAmount { get; set; }
    public int ItemCount { get; set; }
}

public class CartItemDto
{
    public string CartItemId { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImage { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public string Size { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public decimal LineTotal { get; set; }
}

public class AddToCartDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public string? Size { get; set; }
    public string? Color { get; set; }
}

public class UpdateCartItemDto
{
    public string CartItemId { get; set; } = string.Empty;
    public int Quantity { get; set; }
}
