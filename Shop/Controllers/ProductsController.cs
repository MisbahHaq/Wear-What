using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;

namespace Shop.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string GetCurrentUserId()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return userId ?? string.Empty;
        }

        private string? GetCurrentUserName()
        {
            return User.Identity?.Name;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(int? shopId)
        {
            var query = _context.Products.Include(p => p.Shop).AsQueryable();
            if (shopId.HasValue)
                query = query.Where(p => p.ShopId == shopId.Value);
            
            var products = await query.ToListAsync();
            return View(products);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Shop)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null) return NotFound();

            return View(product);
        }

        public async Task<IActionResult> Create()
        {
            var userName = GetCurrentUserName();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
            var currentUserId = user?.Id ?? string.Empty;
            ViewData["ShopId"] = new SelectList(_context.Shops.Where(s => s.OwnerId == currentUserId), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (ModelState.IsValid)
            {
                var shop = await _context.Shops.FindAsync(product.ShopId);
                var currentUserId = GetCurrentUserId();
                if (shop?.OwnerId != currentUserId)
                {
                    return Forbid();
                }
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ShopId"] = new SelectList(_context.Shops, "Id", "Name", product.ShopId);
            return View(product);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            var shop = await _context.Shops.FindAsync(product.ShopId);
            var currentUserId = GetCurrentUserId();
            if (shop?.OwnerId != currentUserId)
            {
                return Forbid();
            }

            ViewData["ShopId"] = new SelectList(_context.Shops.Where(s => s.OwnerId == currentUserId), "Id", "Name", product.ShopId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (id != product.Id) return NotFound();

            var shop = await _context.Shops.FindAsync(product.ShopId);
            var currentUserId = GetCurrentUserId();
            if (shop?.OwnerId != currentUserId)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Attach(product).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ShopId"] = new SelectList(_context.Shops, "Id", "Name", product.ShopId);
            return View(product);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Shop)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null) return NotFound();

            var shop = await _context.Shops.FindAsync(product.ShopId);
            var currentUserId = GetCurrentUserId();
            if (shop?.OwnerId != currentUserId)
            {
                return Forbid();
            }

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                var wishlistItems = _context.WishlistItems.Where(w => w.ProductId == product.Id);
                _context.WishlistItems.RemoveRange(wishlistItems);
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}