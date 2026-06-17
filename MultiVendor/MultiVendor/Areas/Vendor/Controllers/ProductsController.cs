using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MultiVendor.Data;
using MultiVendor.DTOs;
using MultiVendor.Models;

namespace MultiVendor.Areas.Vendor.Controllers;

[Area("Vendor")]
[Authorize(Roles = "Vendor,Admin")]
public class ProductsController : Controller
{
    private readonly ECommerceDbContext _context;

    public ProductsController(ECommerceDbContext context)
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

        var products = await _context.Products
            .Where(p => p.ShopId == shopId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);

        return View(products);
    }

    [HttpGet("create")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProductDto dto, CancellationToken ct = default)
    {
        var shopId = await GetUserShopIdAsync(ct);
        if (shopId is null) return Unauthorized();

        if (!ModelState.IsValid) return View(dto);

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

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
    {
        var shopId = await GetUserShopIdAsync(ct);
        if (shopId is null) return Unauthorized();

        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.ShopId == shopId, ct);

        if (product is null) return NotFound();

        var dto = new UpdateProductDto
        {
            Name = product.Name,
            ShortDescription = product.ShortDescription,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            CoverImageUrl = product.CoverImageUrl,
            Category = product.Category
        };

        ViewBag.ProductId = product.Id;
        return View(dto);
    }

    [HttpPost("edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateProductDto dto, CancellationToken ct = default)
    {
        var shopId = await GetUserShopIdAsync(ct);
        if (shopId is null) return Unauthorized();

        if (!ModelState.IsValid) return View(dto);

        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.ShopId == shopId, ct);

        if (product is null) return NotFound();

        product.Name = dto.Name ?? product.Name;
        product.ShortDescription = dto.ShortDescription;
        product.Description = dto.Description;
        product.Price = dto.Price ?? product.Price;
        product.Stock = dto.Stock ?? product.Stock;
        product.CoverImageUrl = dto.CoverImageUrl;
        product.Category = dto.Category;
        product.IsActive = dto.IsActive ?? product.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        var shopId = await GetUserShopIdAsync(ct);
        if (shopId is null) return Unauthorized();

        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.ShopId == shopId, ct);

        if (product is null) return NotFound();

        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        return RedirectToAction(nameof(Index));
    }
}
