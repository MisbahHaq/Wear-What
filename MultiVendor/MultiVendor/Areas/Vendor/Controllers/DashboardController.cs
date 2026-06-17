using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MultiVendor.Data;

namespace MultiVendor.Areas.Vendor.Controllers;

[Area("Vendor")]
[Authorize(Roles = "Vendor,Admin")]
public class DashboardController : Controller
{
    private readonly ECommerceDbContext _context;

    public DashboardController(ECommerceDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return Unauthorized();

        var shops = await _context.Shops
            .Where(s => s.OwnerId == userId)
            .ToListAsync(ct);

        return View(shops);
    }
}
