using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MultiVendor.DTOs;
using MultiVendor.Data;
using MultiVendor.Models;

namespace MultiVendor.Controllers;

[ApiController]
[Route("api/products")]
public class ProductController : Controller
{
    private readonly ECommerceDbContext _context;

    public ProductController(ECommerceDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAllProducts(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? category = null,
        CancellationToken ct = default)
    {
        var skip = (page - 1) * pageSize;
        var query = _context.Products
            .Where(p => p.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(p => p.Category == category);
        }

        var products = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
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
                Category = p.Category,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync(ct);

        return Ok(products);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetProductById(int id, CancellationToken ct)
    {
        var product = await _context.Products
            .Where(p => p.Id == id && p.IsActive)
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
                Category = p.Category,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt
            })
            .FirstOrDefaultAsync(ct);

        if (product is null) return NotFound();

        return Ok(product);
    }

    [HttpGet("categories")]
    public async Task<ActionResult<IEnumerable<string>>> GetCategories(CancellationToken ct = default)
    {
        var categories = await _context.Products
            .Where(p => p.IsActive && !string.IsNullOrEmpty(p.Category))
            .Select(p => p.Category!)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync(ct);

        return Ok(categories);
    }

    [HttpGet("~/Products/{id:int}")]
    public async Task<IActionResult> Details(int id, CancellationToken ct = default)
    {
        var product = await _context.Products
            .Where(p => p.Id == id && p.IsActive)
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
                Category = p.Category,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt
            })
            .FirstOrDefaultAsync(ct);

        if (product is null) return NotFound();

        return View(product);
    }
}
