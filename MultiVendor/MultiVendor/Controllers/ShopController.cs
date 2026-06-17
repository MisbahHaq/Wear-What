using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MultiVendor.DTOs;
using MultiVendor.Models;
using MultiVendor.Data;
using MultiVendor.Services.Interfaces;

namespace MultiVendor.Controllers;

[ApiController]
[Route("api/shops")]
public class ShopController : Controller
{
    private readonly IFollowService _followService;
    private readonly ECommerceDbContext _context;

    public ShopController(IFollowService followService, ECommerceDbContext context)
    {
        _followService = followService;
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ShopDto>>> GetAllShops(CancellationToken ct)
    {
        var shops = await _context.Shops
            .Where(s => s.IsActive)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new ShopDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                LogoUrl = s.LogoUrl,
                BannerUrl = s.BannerUrl,
                OwnerId = s.OwnerId,
                OwnerName = $"{s.Owner.FirstName} {s.Owner.LastName}",
                FollowerCount = s.Followers.Count,
                ProductCount = s.Products.Count,
                IsFollowing = false,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync(ct);

        return Ok(shops);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ShopDto>> GetShopById(int id, CancellationToken ct)
    {
        var shop = await _context.Shops
            .Where(s => s.Id == id && s.IsActive)
            .Select(s => new ShopDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                LogoUrl = s.LogoUrl,
                BannerUrl = s.BannerUrl,
                OwnerId = s.OwnerId,
                OwnerName = $"{s.Owner.FirstName} {s.Owner.LastName}",
                FollowerCount = s.Followers.Count,
                ProductCount = s.Products.Count,
                IsFollowing = false,
                CreatedAt = s.CreatedAt
            })
            .FirstOrDefaultAsync(ct);

        if (shop is null) return NotFound();

        return Ok(shop);
    }

    [HttpGet("{id:int}/products")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetShopProducts(
        int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var shopExists = await _context.Shops.AnyAsync(s => s.Id == id && s.IsActive, ct);
        if (!shopExists) return NotFound("Shop not found.");

        var skip = (page - 1) * pageSize;

        var products = await _context.Products
            .Where(p => p.ShopId == id && p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
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
                Category = p.Category,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync(ct);

        return Ok(products);
    }

    [Authorize]
    [HttpPost("{id:int}/follow")]
    public async Task<IActionResult> FollowShop(int id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return Unauthorized();

        var shopExists = await _context.Shops.AnyAsync(s => s.Id == id && s.IsActive);
        if (!shopExists) return NotFound("Shop not found.");

        var followed = await _followService.FollowShopAsync(userId, id);

        if (!followed)
            return BadRequest("You are already following this shop.");

        return Ok(new { message = "Successfully followed the shop." });
    }

    [Authorize]
    [HttpDelete("{id:int}/follow")]
    public async Task<IActionResult> UnfollowShop(int id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return Unauthorized();

        var unfollowed = await _followService.UnfollowShopAsync(userId, id);

        if (!unfollowed)
            return BadRequest("You are not following this shop.");

        return NoContent();
    }

    [HttpGet("featured")]
    public async Task<ActionResult<IEnumerable<ShopDto>>> GetFeaturedShops(
        [FromQuery] int count = 10, CancellationToken ct = default)
    {
        var shops = await _context.Shops
            .Where(s => s.IsActive)
            .OrderByDescending(s => s.Followers.Count)
            .Take(count)
            .Select(s => new ShopDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                LogoUrl = s.LogoUrl,
                BannerUrl = s.BannerUrl,
                OwnerId = s.OwnerId,
                OwnerName = $"{s.Owner.FirstName} {s.Owner.LastName}",
                FollowerCount = s.Followers.Count,
                ProductCount = s.Products.Count,
                IsFollowing = false,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync(ct);

        return Ok(shops);
    }

    [HttpGet("~/Shops/{id:int}/Products")]
    public async Task<IActionResult> GetProducts(int id, CancellationToken ct = default)
    {
        var shop = await _context.Shops
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
                CreatedAt = s.CreatedAt
            })
            .FirstOrDefaultAsync(ct);

        if (shop is null) return NotFound();

        var products = await _context.Products
            .Where(p => p.ShopId == id && p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
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
                Category = p.Category,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync(ct);

        ViewBag.Shop = shop;
        return View("GetProducts", products);
    }
}
