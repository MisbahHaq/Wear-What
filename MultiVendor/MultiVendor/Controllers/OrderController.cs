using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiVendor.Data;
using System.Security.Claims;

namespace MultiVendor.Controllers;

[Authorize]
public class OrderController : Controller
{
    private readonly ECommerceDbContext _context;

    public OrderController(ECommerceDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        return RedirectToAction(nameof(CartController.MyOrders), "Cart");
    }

    public async Task<IActionResult> Details(int id, CancellationToken ct = default)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
        {
            return Unauthorized();
        }

        var order = await _context.Orders
            .AsNoTracking()
            .Where(o => o.Id == id && o.CustomerId == userId)
            .Include(o => o.ShopOrders)
            .ThenInclude(so => so.Shop)
            .Include(o => o.ShopOrders)
            .ThenInclude(so => so.OrderItems)
            .SingleOrDefaultAsync(ct);

        if (order is null)
        {
            return NotFound();
        }

        return View(order);
    }
}
