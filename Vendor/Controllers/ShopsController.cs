using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vendor.Data;
using Vendor.Models;

namespace Vendor.Controllers;

[Authorize]
public class ShopsController : Controller
{
    private readonly AppDbContext _db;

    public ShopsController(AppDbContext db)
    {
        _db = db;
    }

    private string CurrentUserId => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";

    public async Task<IActionResult> Index()
    {
        var shops = await _db.Shops
            .Where(s => s.OwnerId == CurrentUserId)
            .ToListAsync();
        return View(shops);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Shop shop)
    {
        shop.OwnerId = CurrentUserId;
        shop.CreatedAt = DateTime.UtcNow;

        if (!ModelState.IsValid) return View(shop);

        _db.Shops.Add(shop);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var shop = await _db.Shops.FirstOrDefaultAsync(s => s.Id == id && s.OwnerId == CurrentUserId);
        if (shop == null) return NotFound();
        return View(shop);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Shop updated)
    {
        if (id != updated.Id) return BadRequest();

        var shop = await _db.Shops.FirstOrDefaultAsync(s => s.Id == id && s.OwnerId == CurrentUserId);
        if (shop == null) return NotFound();

        shop.Name = updated.Name;
        shop.Description = updated.Description;
        shop.Location = updated.Location;

        if (!ModelState.IsValid) return View(shop);

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var shop = await _db.Shops.FirstOrDefaultAsync(s => s.Id == id && s.OwnerId == CurrentUserId);
        if (shop == null) return NotFound();
        return View(shop);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var shop = await _db.Shops.FirstOrDefaultAsync(s => s.Id == id && s.OwnerId == CurrentUserId);
        if (shop == null) return NotFound();

        _db.Shops.Remove(shop);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
