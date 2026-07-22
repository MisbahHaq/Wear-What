using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiVendor.Core.Data;
using MultiVendor.Core.Models;

namespace MultiVendor.Backoffice.Areas.Vendor.Controllers
{
    [Authorize(Roles = "Vendor")]
    public class ShopsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ShopsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private string GetCurrentUserId() => _userManager.GetUserId(User) ?? string.Empty;

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ShopItem shop)
        {
            if (ModelState.IsValid)
            {
                shop.OwnerId = GetCurrentUserId();
                shop.Status = "Approved";
                _context.Add(shop);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }
            return View(shop);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var shop = await _context.Shops.FindAsync(id);
            if (shop == null) return NotFound();

            if (shop.OwnerId != GetCurrentUserId())
            {
                return Forbid();
            }

            return View(shop);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ShopItem shop)
        {
            if (id != shop.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingShop = await _context.Shops.FindAsync(id);
                    if (existingShop == null) return NotFound();

                    existingShop.Name = shop.Name;
                    existingShop.Description = shop.Description;
                    existingShop.ProfileBannerUrl = shop.ProfileBannerUrl;
                    existingShop.ProfileImageUrl = shop.ProfileImageUrl;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ShopExists(shop.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction("Index", "Home");
            }
            return View(shop);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var shop = await _context.Shops
                .Include(s => s.Products)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (shop == null) return NotFound();

            if (shop.OwnerId != GetCurrentUserId())
            {
                return Forbid();
            }

            return View(shop);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var shop = await _context.Shops
                .Include(s => s.Products)
                .ThenInclude(p => p.Comments)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (shop != null)
            {
                foreach (var product in shop.Products)
                {
                    _context.ProductComments.RemoveRange(product.Comments);
                    var wishlistItems = _context.WishlistItems.Where(w => w.ProductId == product.Id);
                    _context.WishlistItems.RemoveRange(wishlistItems);
                }
                _context.Shops.Remove(shop);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index", "Home");
        }

        private bool ShopExists(int id)
        {
            return _context.Shops.Any(e => e.Id == id);
        }
    }
}
