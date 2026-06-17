using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MultiVendor.Areas.Vendor.Controllers;
using MultiVendor.Data;
using MultiVendor.DTOs;
using MultiVendor.Models;
using System.Security.Claims;

[Area("Vendor")]
[Authorize(Roles = "Vendor,Admin")]
public class ProfileController : Controller
{
    private readonly ECommerceDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProfileController(ECommerceDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    private async Task<int?> GetUserShopIdAsync(CancellationToken ct = default)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
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
        if (shopId is null)
        {
            ViewBag.NoShop = true;
            return View();
        }

        var shop = await _context.Shops
            .Where(s => s.Id == shopId)
            .Select(s => new ShopProfileViewModel
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                LogoUrl = s.LogoUrl,
                CreatedAt = s.CreatedAt,
                FollowerCount = s.Followers.Count,
                ProductCount = s.Products.Count
            })
            .FirstOrDefaultAsync(ct);

        var products = await _context.Products
            .Where(p => p.ShopId == shopId)
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

        var vm = new VendorProfilePageViewModel
        {
            Shop = shop,
            Products = products
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddProduct(CreateProductDto dto, CancellationToken ct = default)
    {
        var shopId = await GetUserShopIdAsync(ct);
        if (shopId is null) return Unauthorized();

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please fix the errors in the form.";
            return RedirectToAction(nameof(Index));
        }

        var product = new Product
        {
            ShopId = shopId.Value,
            Name = dto.Name,
            ShortDescription = dto.ShortDescription,
            Description = dto.Description,
            Price = dto.Price,
            Stock = dto.Stock,
            CoverImageUrl = dto.CoverImageUrl,
            Sku = dto.Sku,
            Category = dto.Category,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync(ct);

        TempData["Success"] = $"Product \"{product.Name}\" added successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProduct(int id, CancellationToken ct = default)
    {
        var shopId = await GetUserShopIdAsync(ct);
        if (shopId is null) return Unauthorized();

        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.ShopId == shopId, ct);

        if (product is null) return NotFound();

        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        TempData["Success"] = $"Product \"{product.Name}\" removed.";
        return RedirectToAction(nameof(Index));
    }
}

public class ShopProfileViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public int FollowerCount { get; set; }
    public int ProductCount { get; set; }
}

public class VendorProfilePageViewModel
{
    public ShopProfileViewModel? Shop { get; set; }
    public List<ProductDto> Products { get; set; } = new();
}
