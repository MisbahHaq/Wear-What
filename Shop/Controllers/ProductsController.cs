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
        public async Task<IActionResult> Index(int? shopId, int? categoryId)
        {
            var query = _context.Products.Include(p => p.Shop).Include(p => p.Category).AsQueryable();
            if (shopId.HasValue)
                query = query.Where(p => p.ShopId == shopId.Value);
            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);
            
            var products = await query.ToListAsync();
            var shops = await _context.Shops.ToListAsync();
            ViewBag.Shops = shops;
            ViewBag.SelectedShopId = shopId;
            ViewBag.CategoryId = new SelectList(_context.ShopCategories, "Id", "Name", categoryId);
            return View(products);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Shop)
                .Include(p => p.Comments!)
                .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null) return NotFound();

            return View(product);
        }

        public async Task<IActionResult> Create()
        {
            var userName = GetCurrentUserName();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
            var currentUserId = user?.Id ?? string.Empty;
            var shops = await _context.Shops.Where(s => s.OwnerId == currentUserId).ToListAsync();
            ViewBag.ShopId = new SelectList(shops, "Id", "Name");
            var firstShopId = shops.FirstOrDefault()?.Id;
            if (firstShopId.HasValue)
            {
                ViewBag.CategoryId = new SelectList(_context.ShopCategories.Where(c => c.ShopId == firstShopId.Value), "Id", "Name");
            }
            else
            {
                ViewBag.CategoryId = new SelectList(Enumerable.Empty<ShopCategory>(), "Id", "Name");
            }
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
            var userName = GetCurrentUserName();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
            var currentUserId2 = user?.Id ?? string.Empty;
            ViewBag.ShopId = new SelectList(_context.Shops.Where(s => s.OwnerId == currentUserId2), "Id", "Name", product.ShopId);
            var shopForCategories = await _context.Shops.FindAsync(product.ShopId);
            if (shopForCategories != null)
            {
                ViewBag.CategoryId = new SelectList(_context.ShopCategories.Where(c => c.ShopId == shopForCategories.Id), "Id", "Name", product.CategoryId);
            }
            else
            {
                ViewBag.CategoryId = new SelectList(Enumerable.Empty<ShopCategory>(), "Id", "Name");
            }
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

            ViewBag.ShopId = new SelectList(_context.Shops.Where(s => s.OwnerId == currentUserId), "Id", "Name", product.ShopId);
            ViewBag.CategoryId = new SelectList(_context.ShopCategories.Where(c => c.ShopId == product.ShopId), "Id", "Name", product.CategoryId);
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
                    var existingProduct = await _context.Products.FindAsync(id);
                    if (existingProduct == null) return NotFound();

                    existingProduct.Name = product.Name;
                    existingProduct.Description = product.Description;
                    existingProduct.Price = product.Price;
                    existingProduct.CategoryId = product.CategoryId;
                    existingProduct.Niche = product.Niche;
                    existingProduct.ImageUrl1 = product.ImageUrl1;
                    existingProduct.ImageUrl2 = product.ImageUrl2;
                    existingProduct.ImageUrl3 = product.ImageUrl3;
                    existingProduct.ImageUrl4 = product.ImageUrl4;
                    existingProduct.ImageUrl5 = product.ImageUrl5;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.ShopId = new SelectList(_context.Shops.Where(s => s.OwnerId == currentUserId), "Id", "Name", product.ShopId);
            ViewBag.CategoryId = new SelectList(_context.ShopCategories.Where(c => c.ShopId == product.ShopId), "Id", "Name", product.CategoryId);
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
            var product = await _context.Products
                .Include(p => p.Comments)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product != null)
            {
                _context.ProductComments.RemoveRange(product.Comments);
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