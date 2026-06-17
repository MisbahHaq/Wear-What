using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vendor.Data;
using Vendor.Models;

namespace Vendor.Controllers;

[Authorize]
public class ProductsController : Controller
{
    private readonly AppDbContext _db;

    public ProductsController(AppDbContext db)
    {
        _db = db;
    }

    private string CurrentUserId => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";

    public async Task<IActionResult> Index()
    {
        var products = await _db.Products
            .Include(p => p.Shop)
            .Include(p => p.Category)
            .Where(p => p.Shop!.OwnerId == CurrentUserId)
            .ToListAsync();
        return View(products);
    }

    public async Task<IActionResult> Create()
    {
        await SeedCategories();
        var shops = await _db.Shops.Where(s => s.OwnerId == CurrentUserId).ToListAsync();
        var categories = await _db.ProductCategories.ToListAsync();
        ViewBag.Shops = shops;
        ViewBag.Categories = categories;
        return View(new ProductFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await PopulateFormSelects();
            return View(vm);
        }

        var shop = await _db.Shops.FirstOrDefaultAsync(s => s.Id == vm.ShopId && s.OwnerId == CurrentUserId);
        if (shop == null)
        {
            ModelState.AddModelError("", "Invalid shop.");
            await PopulateFormSelects();
            return View(vm);
        }

        var product = new Product
        {
            ShopId = vm.ShopId,
            Name = vm.Name,
            Description = vm.Description,
            Price = vm.Price,
            Stock = vm.Stock,
            BroadCategory = vm.BroadCategory,
            CategoryId = vm.CategoryId,
            CreatedAt = DateTime.UtcNow
        };

        var urls = new[] { vm.Image1, vm.Image2, vm.Image3, vm.Image4, vm.Image5 }
            .Where(u => !string.IsNullOrWhiteSpace(u))
            .Take(5)
            .ToList();

        product.Images = urls.Select(u => new ProductImage { Url = u! }).ToList();

        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var product = await _db.Products
            .Include(p => p.Images)
            .Include(p => p.Shop)
            .FirstOrDefaultAsync(p => p.Id == id && p.Shop!.OwnerId == CurrentUserId);
        if (product == null) return NotFound();

        await PopulateFormSelects();

        var vm = new ProductFormViewModel
        {
            ShopId = product.ShopId,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            BroadCategory = product.BroadCategory,
            CategoryId = product.CategoryId
        };

        var images = product.Images.OrderBy(i => i.Id).Select(i => i.Url).ToList();
        if (images.Count > 0) vm.Image1 = images.ElementAtOrDefault(0);
        if (images.Count > 1) vm.Image2 = images.ElementAtOrDefault(1);
        if (images.Count > 2) vm.Image3 = images.ElementAtOrDefault(2);
        if (images.Count > 3) vm.Image4 = images.ElementAtOrDefault(3);
        if (images.Count > 4) vm.Image5 = images.ElementAtOrDefault(4);

        ViewBag.ProductId = product.Id;
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductFormViewModel vm)
    {
        if (id <= 0) return BadRequest();

        var product = await _db.Products
            .Include(p => p.Images)
            .Include(p => p.Shop)
            .FirstOrDefaultAsync(p => p.Id == id && p.Shop!.OwnerId == CurrentUserId);
        if (product == null) return NotFound();

        if (!ModelState.IsValid)
        {
            await PopulateFormSelects();
            return View(vm);
        }

        product.Name = vm.Name;
        product.Description = vm.Description;
        product.Price = vm.Price;
        product.Stock = vm.Stock;
        product.BroadCategory = vm.BroadCategory;
        product.CategoryId = vm.CategoryId;

        _db.ProductImages.RemoveRange(product.Images);

        var urls = new[] { vm.Image1, vm.Image2, vm.Image3, vm.Image4, vm.Image5 }
            .Where(u => !string.IsNullOrWhiteSpace(u))
            .Take(5)
            .ToList();

        product.Images = urls.Select(u => new ProductImage { Url = u!, ProductId = product.Id }).ToList();

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var product = await _db.Products
            .Include(p => p.Shop)
            .FirstOrDefaultAsync(p => p.Id == id && p.Shop!.OwnerId == CurrentUserId);
        if (product == null) return NotFound();
        return View(product);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await _db.Products
            .Include(p => p.Shop)
            .FirstOrDefaultAsync(p => p.Id == id && p.Shop!.OwnerId == CurrentUserId);
        if (product == null) return NotFound();

        _db.Products.Remove(product);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task SeedCategories()
    {
        if (!await _db.ProductCategories.AnyAsync())
        {
            var defaults = new[] { "Women", "Men", "Unisex" };
            var broads = new[] { "Women", "Men", "Unisex" };
            var categories = new List<ProductCategory>();

            categories.AddRange(broads.Select(b => new ProductCategory { Name = b }));
            categories.Add(new ProductCategory { Name = "Kitchen" });
            categories.AddRange(defaults.SelectMany(b => new[] { "Clothes", "Toys", "Decor" }.Select(name => new ProductCategory { Name = $"{b} - {name}" })));

            await _db.ProductCategories.AddRangeAsync(categories);
            await _db.SaveChangesAsync();
        }
    }

    private async Task PopulateFormSelects()
    {
        ViewBag.Shops = await _db.Shops.Where(s => s.OwnerId == CurrentUserId).ToListAsync();
        ViewBag.Categories = await _db.ProductCategories.ToListAsync();
    }
}
