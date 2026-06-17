using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vendor.Data;
using Vendor.Models;

namespace Vendor.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly AppDbContext _db;

    public ProfileController(AppDbContext db)
    {
        _db = db;
    }

    private string CurrentUserId => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";

    public async Task<IActionResult> Index()
    {
        var user = await _db.Users
            .Include(u => u.Addresses)
            .Include(u => u.Orders)
                .ThenInclude(o => o.Items)
            .Include(u => u.Orders)
                .ThenInclude(o => o.Cancellation)
            .Include(u => u.WishlistItems)
                .ThenInclude(w => w.Product)
                    .ThenInclude(p => p.Images)
            .FirstOrDefaultAsync(u => u.Id == CurrentUserId);

        if (user == null) return NotFound();

        return View(user);
    }

    public async Task<IActionResult> Settings()
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == CurrentUserId);
        if (user == null) return NotFound();

        var vm = new ProfileSettingsViewModel
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? "",
            PhoneNumber = user.PhoneNumber ?? "",
            ProfileImageUrl = user.ProfileImageUrl
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Settings(ProfileSettingsViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == CurrentUserId);
        if (user == null) return NotFound();

        user.FirstName = vm.FirstName;
        user.LastName = vm.LastName;
        user.Email = vm.Email;
        user.PhoneNumber = vm.PhoneNumber;
        user.ProfileImageUrl = vm.ProfileImageUrl;

        await _db.SaveChangesAsync();
        TempData["Success"] = "Profile updated successfully.";
        return RedirectToAction(nameof(Settings));
    }

    public async Task<IActionResult> Orders()
    {
        var orders = await _db.Orders
            .Include(o => o.Items)
            .Include(o => o.Cancellation)
            .Where(o => o.UserId == CurrentUserId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return View(orders);
    }

    public async Task<IActionResult> Wishlists()
    {
        var items = await _db.WishlistItems
            .Include(w => w.Product)
                .ThenInclude(p => p.Images)
            .Include(w => w.Product)
                .ThenInclude(p => p.Shop)
            .Where(w => w.UserId == CurrentUserId)
            .OrderByDescending(w => w.AddedAt)
            .ToListAsync();

        return View(items);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveFromWishlist(int id)
    {
        var item = await _db.WishlistItems.FirstOrDefaultAsync(w => w.Id == id && w.UserId == CurrentUserId);
        if (item != null)
        {
            _db.WishlistItems.Remove(item);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Wishlists));
    }

    public async Task<IActionResult> Cancellations()
    {
        var cancellations = await _db.Cancellations
            .Include(c => c.Order)
                .ThenInclude(o => o.Items)
            .Where(c => c.Order.UserId == CurrentUserId)
            .OrderByDescending(c => c.RequestedAt)
            .ToListAsync();

        return View(cancellations);
    }
}
