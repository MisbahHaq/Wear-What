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
            .Where(p => p.Shop!.OwnerId == CurrentUserId)
            .ToListAsync();
        return View(products);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Shops = await _db.Shops
            .Where(s => s.OwnerId == CurrentUserId)
            .ToListAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product)
    {
        product.CreatedAt = DateTime.UtcNow;

        if (!ModelState.IsValid)
        {
            ViewBag.Shops = await _db.Shops
                .Where(s => s.OwnerId == CurrentUserId)
                .ToListAsync();
            return View(product);
        }

        var shop = await _db.Shops.FirstOrDefaultAsync(s => s.Id == product.ShopId && s.OwnerId == CurrentUserId);
        if (shop == null)
        {
            ModelState.AddModelError("", "Invalid shop.");
            ViewBag.Shops = await _db.Shops
                .Where(s => s.OwnerId == CurrentUserId)
                .ToListAsync();
            return View(product);
        }

        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var product = await _db.Products
            .Include(p => p.Shop)
            .FirstOrDefaultAsync(p => p.Id == id && p.Shop!.OwnerId == CurrentUserId);
        if (product == null) return NotFound();

        ViewBag.Shops = await _db.Shops
            .Where(s => s.OwnerId == CurrentUserId)
            .ToListAsync();
        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product updated)
    {
        if (id != updated.Id) return BadRequest();

        var product = await _db.Products
            .Include(p => p.Shop)
            .FirstOrDefaultAsync(p => p.Id == id && p.Shop!.OwnerId == CurrentUserId);
        if (product == null) return NotFound();

        product.Name = updated.Name;
        product.Description = updated.Description;
        product.Price = updated.Price;
        product.Stock = updated.Stock;
        product.ImageUrl = updated.ImageUrl;
        product.ShopId = updated.ShopId;

        if (!ModelState.IsValid)
        {
            ViewBag.Shops = await _db.Shops
                .Where(s => s.OwnerId == CurrentUserId)
                .ToListAsync();
            return View(product);
        }

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
}
