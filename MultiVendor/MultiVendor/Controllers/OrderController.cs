using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MultiVendor.DTOs;
using MultiVendor.Services.Interfaces;
using MultiVendor.Data;

namespace MultiVendor.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly ICheckoutService _checkoutService;
    private readonly ECommerceDbContext _context;

    public OrderController(ICheckoutService checkoutService, ECommerceDbContext context)
    {
        _checkoutService = checkoutService;
        _context = context;
    }

    [HttpPost("checkout")]
    public async Task<ActionResult<CheckoutResultDto>> Checkout([FromBody] CheckoutDto checkoutDto)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return Unauthorized();

        var result = await _checkoutService.ProcessCheckoutAsync(userId, checkoutDto);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("my-orders")]
    public async Task<ActionResult<IEnumerable<object>>> GetMyOrders(CancellationToken ct = default)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return Unauthorized();

        var orders = await _context.Orders
            .Where(o => o.CustomerId == userId)
            .OrderByDescending(o => o.OrderDate)
            .Select(o => new
            {
                o.Id,
                o.OrderNumber,
                o.OrderDate,
                o.Status,
                o.GrandTotal,
                o.ShippingAddress,
                o.TrackingNumber,
                o.ShippedAt,
                o.DeliveredAt,
                ShopCount = o.ShopOrders.Count,
                ShopOrders = o.ShopOrders.Select(so => new
                {
                    so.Id,
                    so.VendorOrderNumber,
                    so.Status,
                    so.ShopId,
                    ShopName = so.Shop.Name,
                    so.TrackingNumber,
                    so.ShippedAt,
                    so.DeliveredAt,
                    ItemCount = so.OrderItems.Count
                })
            })
            .ToListAsync(ct);

        return Ok(orders);
    }
}
