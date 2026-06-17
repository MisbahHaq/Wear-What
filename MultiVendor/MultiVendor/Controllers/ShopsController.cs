using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MultiVendor.Data;
using MultiVendor.DTOs;
using MultiVendor.Models;
using System.Security.Claims;

namespace MultiVendor.Controllers;

[Authorize]
public class ShopsController : Controller
{
    private readonly ECommerceDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ShopsController(ECommerceDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateShopDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return Unauthorized();

        if (!user.IsVendor)
        {
            user.IsVendor = true;
            await _userManager.UpdateAsync(user);
            if (!await _userManager.IsInRoleAsync(user, "Vendor"))
            {
                await _userManager.AddToRoleAsync(user, "Vendor");
            }
        }

        var shop = new Shop
        {
            OwnerId = userId,
            Name = dto.Name,
            Description = dto.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Shops.Add(shop);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index", "Home");
    }

    [HttpGet("my-shops")]
    public async Task<IActionResult> MyShops(CancellationToken ct = default)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return Unauthorized();

        var shops = await _context.Shops
            .Where(s => s.OwnerId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new ShopDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                LogoUrl = s.LogoUrl,
                OwnerName = $"{s.Owner.FirstName} {s.Owner.LastName}",
                FollowerCount = s.Followers.Count,
                ProductCount = s.Products.Count,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync(ct);

        return View(shops);
    }
}
