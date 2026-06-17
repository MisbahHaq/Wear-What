using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiVendor.Data;
using MultiVendor.DTOs;
using MultiVendor.Models;
using MultiVendor.Services.Interfaces;
using System.Security.Claims;

namespace MultiVendor.Controllers;

[Authorize]
public class ShopsController : Controller
{
    private readonly ECommerceDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IFollowService _followService;

    public ShopsController(
        ECommerceDbContext context,
        UserManager<ApplicationUser> userManager,
        IFollowService followService)
    {
        _context = context;
        _userManager = userManager;
        _followService = followService;
    }

    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var shops = await _context.Shops
            .AsNoTracking()
            .Where(s => s.IsActive)
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
                IsFollowing = userId != null && _context.Follows.Any(f => f.FollowerId == userId && f.ShopId == s.Id),
                CreatedAt = s.CreatedAt
            })
            .ToListAsync(ct);

        return View(shops);
    }

    public async Task<IActionResult> Details(int id, CancellationToken ct = default)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var shop = await _context.Shops
            .AsNoTracking()
            .Where(s => s.Id == id && s.IsActive)
            .Select(s => new ShopDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                LogoUrl = s.LogoUrl,
                BannerUrl = s.BannerUrl,
                OwnerName = $"{s.Owner.FirstName} {s.Owner.LastName}",
                FollowerCount = s.Followers.Count,
                ProductCount = s.Products.Count,
                IsFollowing = userId != null && _context.Follows.Any(f => f.FollowerId == userId && f.ShopId == s.Id),
                CreatedAt = s.CreatedAt
            })
            .SingleOrDefaultAsync(ct);

        if (shop is null)
        {
            return NotFound();
        }

        var products = await _context.Products
            .AsNoTracking()
            .Where(p => p.ShopId == id && p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                ShopId = p.ShopId,
                ShopName = shop.Name,
                Name = p.Name,
                ShortDescription = p.ShortDescription,
                Price = p.Price,
                Stock = p.Stock,
                CoverImageUrl = p.CoverImageUrl,
                Category = p.Category,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync(ct);

        ViewBag.Products = products;
        return View(shop);
    }

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
            LogoUrl = dto.LogoUrl,
            BannerUrl = dto.BannerUrl,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Shops.Add(shop);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Shop created successfully.";
        return RedirectToAction(nameof(Details), new { id = shop.Id });
    }

    public async Task<IActionResult> MyShops(CancellationToken ct = default)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return Unauthorized();

        var shops = await _context.Shops
            .AsNoTracking()
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Follow(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return Unauthorized();

        var followed = await _followService.FollowShopAsync(userId, id);
        TempData["Success"] = followed ? "You are now following this shop." : "You already follow this shop.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unfollow(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return Unauthorized();

        var unfollowed = await _followService.UnfollowShopAsync(userId, id);
        TempData["Success"] = unfollowed ? "You unfollowed this shop." : "You were not following this shop.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
