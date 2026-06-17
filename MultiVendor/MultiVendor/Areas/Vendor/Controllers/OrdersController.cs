using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MultiVendor.Data;
using MultiVendor.Models;

namespace MultiVendor.Areas.Vendor.Controllers;

[Area("Vendor")]
[Authorize(Roles = "Vendor,Admin")]
public class OrdersController : Controller
{
    private readonly ECommerceDbContext _context;

    public OrdersController(ECommerceDbContext context)
    {
        _context = context;
    }

    private async Task<int?> GetUserShopIdAsync(CancellationToken ct = default)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return null;

        return await _context.Shops
            .Where(s => s.OwnerId == userId)
            .Select(s => (int?)s.Id)
            .FirstOrDefaultAsync(ct);
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var shopId = await GetUserShopIdAsync(ct);
        if (shopId is null) return Unauthorized();

        var vendorOrders = await _context.ShopOrders
            .Where(so => so.ShopId == shopId)
            .OrderByDescending(so => so.CreatedAt)
            .Include(so => so.Order)
            .ThenInclude(o => o.Customer)
            .Select(so => new
            {
                so.Id,
                so.VendorOrderNumber,
                so.Status,
                so.SubTotal,
                so.VendorTotal,
                so.TrackingNumber,
                so.ShippedAt,
                so.DeliveredAt,
                so.CreatedAt,
                OrderId = so.Order.Id,
                CustomerName = $"{so.Order.Customer.FirstName} {so.Order.Customer.LastName}",
                CustomerEmail = so.Order.Customer.Email,
                OrderStatus = so.Order.Status,
                ItemCount = so.OrderItems.Count
            })
            .ToListAsync(ct);

        return View(vendorOrders);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(int id, CancellationToken ct = default)
    {
        var shopId = await GetUserShopIdAsync(ct);
        if (shopId is null) return Unauthorized();

        var shopOrder = await _context.ShopOrders
            .Where(so => so.Id == id && so.ShopId == shopId)
            .Include(so => so.Order)
            .Include(so => so.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(ct);

        if (shopOrder is null) return NotFound();

        var vm = new
        {
            shopOrder.Id,
            shopOrder.VendorOrderNumber,
            shopOrder.Status,
            shopOrder.SubTotal,
            shopOrder.VendorTotal,
            shopOrder.TrackingNumber,
            shopOrder.CreatedAt,
            shopOrder.ShippedAt,
            shopOrder.DeliveredAt,
            ParentOrderId = shopOrder.Order.Id,
            ParentOrderNumber = shopOrder.Order.OrderNumber,
            ParentOrderStatus = shopOrder.Order.Status,
            ShippingAddress = shopOrder.Order.ShippingAddress,
            CustomerName = $"{shopOrder.Order.Customer.FirstName} {shopOrder.Order.Customer.LastName}",
            CustomerEmail = shopOrder.Order.Customer.Email,
            Items = shopOrder.OrderItems.Select(oi => new
            {
                oi.ProductId,
                oi.ProductNameSnapshot,
                oi.ProductImageSnapshot,
                oi.Quantity,
                oi.UnitPrice,
                oi.LineTotal
            }).ToList()
        };

        return View(vm);
    }
}
