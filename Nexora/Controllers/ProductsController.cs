using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexora.Data;
using Nexora.Models;

namespace Nexora.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProductsController(ApplicationDbContext context)
    {
        _context = context;
    }

    private static ProductDto MapToProductDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            Gender = product.Gender,
            ImageUrl = product.ImageUrl,
            ImageUrls = product.ImageUrls,
            Tags = product.Tags,
            Colors = product.Colors
        };
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts([FromQuery] string? gender)
    {
        var products = _context.Products.AsQueryable();
        if (!string.IsNullOrEmpty(gender))
        {
            products = products.Where(p => p.Gender == gender);
        }

        var productDtos = await products
            .Select(p => MapToProductDto(p))
            .ToListAsync();

        return Ok(productDtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product == null)
        {
            return NotFound();
        }

        return Ok(MapToProductDto(product));
    }

    [HttpGet("{id}/related")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetRelatedProducts(int id)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product == null)
        {
            return NotFound();
        }

        var related = new List<Product>();
        if (!string.IsNullOrEmpty(product.Tags))
        {
            var currentTags = product.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrEmpty(t))
                .ToList();

            if (currentTags.Any())
            {
                var candidates = await _context.Products
                    .Where(p => p.Id != product.Id && p.Tags != null)
                    .ToListAsync();

                related = candidates.AsEnumerable()
                    .Where(p => p.GetTags() != null && p.GetTags()!.Any(t => currentTags.Contains(t)))
                    .ToList();
            }
        }

        return Ok(related.Select(MapToProductDto).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductDto createDto)
    {
        var product = new Product
        {
            Name = createDto.Name,
            Description = createDto.Description,
            Price = createDto.Price,
            Stock = createDto.Stock,
            Gender = createDto.Gender,
            ImageUrl = createDto.ImageUrl,
            ImageUrls = createDto.ImageUrls,
            Tags = createDto.Tags,
            Colors = createDto.Colors
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, MapToProductDto(product));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto updateDto)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        if (updateDto.Name != null) product.Name = updateDto.Name;
        if (updateDto.Description != null) product.Description = updateDto.Description;
        if (updateDto.Price.HasValue) product.Price = updateDto.Price.Value;
        if (updateDto.Stock.HasValue) product.Stock = updateDto.Stock.Value;
        product.Gender = updateDto.Gender ?? product.Gender;
        product.ImageUrl = updateDto.ImageUrl ?? product.ImageUrl;
        product.ImageUrls = updateDto.ImageUrls ?? product.ImageUrls;
        product.Tags = updateDto.Tags ?? product.Tags;
        product.Colors = updateDto.Colors ?? product.Colors;

        _context.Update(product);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("~/Products/Details/{id}")]
    public async Task<IActionResult> Details(int id)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product == null)
        {
            return NotFound();
        }

        return View(product);
    }

    [HttpGet("~/Products")]
    public async Task<IActionResult> AdminIndex()
    {
        var products = await _context.Products.ToListAsync();
        return View(products);
    }

    [HttpGet("~/Products/Create")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost("~/Products/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] CreateProductDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var product = new Product
        {
            Name = model.Name,
            Description = model.Description,
            Price = model.Price,
            Stock = model.Stock,
            Gender = model.Gender,
            ImageUrl = model.ImageUrl,
            ImageUrls = model.ImageUrls,
            Tags = model.Tags,
            Colors = model.Colors
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(AdminIndex));
    }

    [HttpGet("~/Products/Edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        var model = new UpdateProductDto
        {
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            Gender = product.Gender,
            ImageUrl = product.ImageUrl,
            ImageUrls = product.ImageUrls,
            Tags = product.Tags,
            Colors = product.Colors
        };

        ViewBag.ProductId = product.Id;
        return View(model);
    }

    [HttpPost("~/Products/Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [FromForm] UpdateProductDto model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ProductId = id;
            return View(model);
        }

        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        product.Name = model.Name ?? product.Name;
        product.Description = model.Description ?? product.Description;
        product.Price = model.Price ?? product.Price;
        product.Stock = model.Stock ?? product.Stock;
        product.Gender = model.Gender ?? product.Gender;
        product.ImageUrl = model.ImageUrl ?? product.ImageUrl;
        product.ImageUrls = model.ImageUrls ?? product.ImageUrls;
        product.Tags = model.Tags ?? product.Tags;
        product.Colors = model.Colors ?? product.Colors;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(AdminIndex));
    }

    [HttpGet("~/Products/Delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        return View(product);
    }

    [HttpPost("~/Products/Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(AdminIndex));
    }
}
