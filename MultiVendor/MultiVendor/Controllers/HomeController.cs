using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using MultiVendor.Data;
using MultiVendor.DTOs;
using MultiVendor.Models;
using MultiVendor.Services.Interfaces;
using MultiVendor.ViewModels;

namespace MultiVendor.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ECommerceDbContext _context;
    private readonly IFollowService _followService;

    public HomeController(
        ILogger<HomeController> logger,
        ECommerceDbContext context,
        IFollowService followService)
    {
        _logger = logger;
        _context = context;
        _followService = followService;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var featuredShops = await _context.Shops
            .Where(s => s.IsActive)
            .OrderByDescending(s => s.Followers.Count)
            .Take(6)
            .Select(s => new ShopDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                LogoUrl = s.LogoUrl,
                OwnerName = $"{s.Owner.FirstName} {s.Owner.LastName}",
                FollowerCount = s.Followers.Count,
                ProductCount = s.Products.Count,
                IsFollowing = userId != null && _context.Follows.Any(f => f.FollowerId == userId && f.ShopId == s.Id)
            })
            .ToListAsync(ct);

        var newArrivals = await _context.Products
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .Take(12)
            .Include(p => p.Shop)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                ShopId = p.ShopId,
                ShopName = p.Shop.Name,
                Name = p.Name,
                ShortDescription = p.ShortDescription,
                Price = p.Price,
                Stock = p.Stock,
                CoverImageUrl = p.CoverImageUrl,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync(ct);

        var vm = new HomePageViewModel
        {
            FeaturedShops = featuredShops,
            NewArrivals = newArrivals
        };

        return View(vm);
    }

    public async Task<IActionResult> BrowseShops(CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var shops = await _context.Shops
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

    public async Task<IActionResult> FollowedFeed(int page = 1, int pageSize = 24, CancellationToken ct = default)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
        {
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("FollowedFeed", "Home") });
        }

        var products = await _followService.GetFollowedShopsFeedAsync(userId, page, pageSize);
        ViewBag.CurrentPage = Math.Max(1, page);
        ViewBag.TotalPages = Math.Max(1, (int)Math.Ceiling(products.Count() / (double)Math.Clamp(pageSize, 1, 48)));
        return View(products);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

