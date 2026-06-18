using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;

namespace Shop.Controllers
{
    [Authorize]
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

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var shops = await _context.Shops.Include(s => s.Products).ToListAsync();
            return View(shops);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var shop = await _context.Shops
                .Include(s => s.Products)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (shop == null) return NotFound();

            return View(shop);
        }

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
                _context.Add(shop);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(shop);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var shop = await _context.Shops.FindAsync(id);
            if (shop == null) return NotFound();

            if (shop.OwnerId != GetCurrentUserId() && !User.IsInRole("Admin"))
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
                    _context.Attach(shop).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ShopExists(shop.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(shop);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var shop = await _context.Shops
                .FirstOrDefaultAsync(m => m.Id == id);
            if (shop == null) return NotFound();

            if (shop.OwnerId != GetCurrentUserId() && !User.IsInRole("Admin"))
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
                .FirstOrDefaultAsync(s => s.Id == id);
            if (shop != null)
            {
                foreach (var product in shop.Products)
                {
                    var wishlistItems = _context.WishlistItems.Where(w => w.ProductId == product.Id);
                    _context.WishlistItems.RemoveRange(wishlistItems);
                }
                _context.Shops.Remove(shop);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ShopExists(int id)
        {
            return _context.Shops.Any(e => e.Id == id);
        }
    }
}