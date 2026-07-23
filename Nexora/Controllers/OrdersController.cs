using Microsoft.AspNetCore.Mvc;
using Nexora.Data;
using Nexora.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Nexora.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public OrdersController(ApplicationDbContext context)
    {
        _context = context;
    }

    private static OrderDto MapToOrderDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            UserEmail = order.UserEmail,
            Address = order.Address,
            Country = order.Country,
            City = order.City,
            PhoneNumber = order.PhoneNumber,
            OrderDate = order.OrderDate,
            Status = order.Status,
            DeliveryMethod = order.DeliveryMethod,
            PaymentMethod = order.PaymentMethod,
            TotalAmount = order.TotalAmount,
            Items = order.Items.Select(MapToOrderItemDto).ToList()
        };
    }

    private static OrderItemDto MapToOrderItemDto(OrderItem item)
    {
        return new OrderItemDto
        {
            Id = item.Id,
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            Quantity = item.Quantity,
            Price = item.Price,
            ImageUrl = item.ImageUrl,
            Size = item.Size,
            Color = item.Color
        };
    }

    private List<Models.CartItem>? GetCart()
    {
        var cartJson = HttpContext.Session.GetString("Cart");
        if (string.IsNullOrEmpty(cartJson))
        {
            return null;
        }

        return JsonSerializer.Deserialize<List<Models.CartItem>>(cartJson) ?? new List<Models.CartItem>();
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders()
    {
        var userEmail = HttpContext.Session.GetString("UserEmail");
        var isAdmin = HttpContext.Session.GetString("IsAdmin") == "true";

        if (isAdmin)
        {
            var orders = await _context.Orders
                .Include(o => o.Items)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return Ok(orders.Select(MapToOrderDto).ToList());
        }

        if (string.IsNullOrEmpty(userEmail))
        {
            return Unauthorized();
        }

        var userOrders = await _context.Orders
            .Include(o => o.Items)
            .Where(o => o.UserEmail == userEmail)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return Ok(userOrders.Select(MapToOrderDto).ToList());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrder(int id)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return NotFound();
        }

        var userEmail = HttpContext.Session.GetString("UserEmail");
        var isAdmin = HttpContext.Session.GetString("IsAdmin") == "true";

        if (!isAdmin && order.UserEmail != userEmail)
        {
            return Unauthorized();
        }

        return Ok(MapToOrderDto(order));
    }

    [HttpPost("checkout")]
    public async Task<ActionResult<OrderDto>> Checkout([FromBody] CheckoutDto checkoutDto)
    {
        var userEmail = HttpContext.Session.GetString("UserEmail");
        if (string.IsNullOrEmpty(userEmail))
        {
            return Unauthorized(new { message = "Please log in to checkout." });
        }

        var cartItems = GetCart();
        if (cartItems == null || !cartItems.Any())
        {
            return BadRequest(new { message = "Cart is empty." });
        }

        var productIds = cartItems.Select(item => item.ProductId).ToList();
        var products = await _context.Products.Where(p => productIds.Contains(p.Id)).ToListAsync();

        var subtotal = cartItems.Sum(item =>
        {
            var product = products.FirstOrDefault(p => p.Id == item.ProductId);
            return product?.Price * item.Quantity ?? 0;
        });

        var deliveryFee = checkoutDto.DeliveryMethod == "express" ? 500m : 0m;
        var paymentFee = checkoutDto.PaymentMethod == "cod" ? 800m : 0m;

        var order = new Order
        {
            UserEmail = userEmail,
            Address = checkoutDto.Address,
            Country = checkoutDto.Country,
            City = checkoutDto.City,
            PhoneNumber = checkoutDto.PhoneNumber,
            OrderDate = DateTime.UtcNow,
            Status = "Pending",
            DeliveryMethod = checkoutDto.DeliveryMethod,
            PaymentMethod = checkoutDto.PaymentMethod,
            TotalAmount = subtotal + deliveryFee + paymentFee,
            Items = new List<OrderItem>()
        };

        foreach (var cartItem in cartItems)
        {
            var product = products.FirstOrDefault(p => p.Id == cartItem.ProductId);
            if (product != null)
            {
                order.Items.Add(new OrderItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Quantity = cartItem.Quantity,
                    Price = product.Price,
                    ImageUrl = product.ImageUrl,
                    Size = cartItem.Size,
                    Color = cartItem.Color
                });
            }
        }

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
        if (user != null)
        {
            user.Address = checkoutDto.Address;
            await _context.SaveChangesAsync();
        }

        HttpContext.Session.Remove("Cart");

        var dbCartItems = _context.ShoppingCarts.Where(c => c.UserEmail == userEmail);
        _context.ShoppingCarts.RemoveRange(dbCartItems);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, MapToOrderDto(order));
    }

    [HttpPost("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto statusDto)
    {
        if (HttpContext.Session.GetString("IsAdmin") != "true")
        {
            return Unauthorized(new { message = "Admin access required." });
        }

        var order = await _context.Orders.FindAsync(id);
        if (order == null)
        {
            return NotFound(new { message = "Order not found." });
        }

        var validStatuses = new[] { "Pending", "Order Received", "Out for Delivery", "Delivered", "Cancelled" };
        if (!validStatuses.Contains(statusDto.Status))
        {
            return BadRequest(new { message = "Invalid status." });
        }

        order.Status = statusDto.Status;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Status updated." });
    }
}
