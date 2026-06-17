using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiVendor.Data;
using MultiVendor.DTOs;

namespace MultiVendor.Controllers;

public class ProductController : Controller
{
    private readonly ECommerceDbContext _context;

    public ProductController(ECommerceDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(
        string? search,
        string? category,
        int? shopId,
        int page = 1,
        int pageSize = 24,
        CancellationToken ct = default)
    {
        var safePage = Math.Max(1, page);
        var safePageSize = Math.Clamp(pageSize, 1, 48);
        var skip = (safePage - 1) * safePageSize;

        var query = _context.Products
            .AsNoTracking()
            .Where(p => p.IsActive)
            .Include(p => p.Shop)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.Name.Contains(search) || (p.ShortDescription ?? string.Empty).Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(p => p.Category == category);
        }

        if (shopId.HasValue)
        {
            query = query.Where(p => p.ShopId == shopId.Value);
        }

        var categories = await _context.Products
            .AsNoTracking()
            .Where(p => p.IsActive && !string.IsNullOrWhiteSpace(p.Category))
            .Select(p => p.Category!)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync(ct);

        var total = await query.CountAsync(ct);
        var products = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(safePageSize)
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

        ViewBag.Search = search;
        ViewBag.Category = category;
        ViewBag.ShopId = shopId;
        ViewBag.TotalPages = (int)Math.Ceiling(total / (double)safePageSize);
        ViewBag.CurrentPage = safePage;
        ViewBag.Categories = categories;

        return View(products);
    }

    public async Task<IActionResult> Details(int id, CancellationToken ct = default)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Where(p => p.Id == id && p.IsActive)
            .Include(p => p.Shop)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                ShopId = p.ShopId,
                ShopName = p.Shop.Name,
                Name = p.Name,
                ShortDescription = p.ShortDescription,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                CoverImageUrl = p.CoverImageUrl,
                Category = p.Category,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt
            })
            .SingleOrDefaultAsync(ct);

        if (product is null)
        {
            return NotFound();
        }

        return View(product);
    }
}
