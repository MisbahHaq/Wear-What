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
        public async Task<IActionResult> Index(int? categoryId, string? searchString)
        {
            var query = _context.Products.Include(p => p.Shop).Include(p => p.Category).AsQueryable();
            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);
            if (!string.IsNullOrWhiteSpace(searchString))
                query = query.Where(p => p.Name.Contains(searchString) || p.Description.Contains(searchString) || (p.Shop != null && p.Shop.Name.Contains(searchString)));

            var products = await query.OrderByDescending(p => p.Id).ToListAsync();
            ViewBag.CategoryId = new SelectList(_context.ShopCategories, "Id", "Name", categoryId);
            ViewBag.SearchString = searchString;
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
                .Include(p => p.Specifications)
                .Include(p => p.Colors)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null) return NotFound();

            var currentUserId = GetCurrentUserId();
            ViewBag.IsWishlisted = currentUserId != null && await _context.WishlistItems.AnyAsync(w => w.UserId == currentUserId && w.ProductId == product.Id);

            return View(product);
        }

        public async Task<IActionResult> Create()
        {
            var userName = GetCurrentUserName();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
            var currentUserId = user?.Id ?? string.Empty;
            var shop = await _context.Shops.FirstOrDefaultAsync(s => s.OwnerId == currentUserId);
            if (shop != null)
            {
                ViewBag.ShopId = shop.Id;
            }
            ViewBag.CategoryId = new SelectList(_context.ShopCategories, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, string[]? specKeys, string[]? specValues, string[]? colors)
        {
            if (ModelState.IsValid)
            {
                if (product.ShopId == 0)
                {
                    var userName = GetCurrentUserName();
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
                    var userId = user?.Id ?? string.Empty;
                    var userShop = await _context.Shops.FirstOrDefaultAsync(s => s.OwnerId == userId);
                    if (userShop == null)
                    {
                        ModelState.AddModelError("", "You must create a shop before adding products.");
                        ViewBag.CategoryId = new SelectList(_context.ShopCategories, "Id", "Name");
                        return View(product);
                    }
                    product.ShopId = userShop.Id;
                }

                var shop = await _context.Shops.FindAsync(product.ShopId);
                var currentUserId = GetCurrentUserId();
                if (shop?.OwnerId != currentUserId)
                {
                    return Forbid();
                }
                _context.Add(product);
                await _context.SaveChangesAsync();

                if (specKeys != null && specValues != null && specKeys.Length > 0)
                {
                    var specs = new List<ProductSpecification>();
                    for (int i = 0; i < specKeys.Length; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(specKeys[i]) && !string.IsNullOrWhiteSpace(specValues[i]))
                        {
                            specs.Add(new ProductSpecification
                            {
                                ProductId = product.Id,
                                Key = specKeys[i].Trim(),
                                Value = specValues[i].Trim()
                            });
                        }
                    }
                    if (specs.Any())
                    {
                        _context.ProductSpecifications.AddRange(specs);
                        await _context.SaveChangesAsync();
                    }
                }

                if (colors != null && colors.Length > 0)
                {
                    var productColors = new List<ProductColor>();
                    foreach (var color in colors)
                    {
                        if (!string.IsNullOrWhiteSpace(color))
                        {
                            productColors.Add(new ProductColor
                            {
                                ProductId = product.Id,
                                ColorName = color.Trim()
                            });
                        }
                    }
                    if (productColors.Any())
                    {
                        _context.ProductColors.AddRange(productColors);
                        await _context.SaveChangesAsync();
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            ViewBag.CategoryId = new SelectList(_context.ShopCategories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Specifications)
                .Include(p => p.Colors)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();

            var shop = await _context.Shops.FindAsync(product.ShopId);
            var currentUserId = GetCurrentUserId();
            if (shop?.OwnerId != currentUserId)
            {
                return Forbid();
            }

            ViewBag.CategoryId = new SelectList(_context.ShopCategories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, string[]? specKeys, string[]? specValues, string[]? colors)
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
                    var existingProduct = await _context.Products
                        .Include(p => p.Specifications)
                        .Include(p => p.Colors)
                        .FirstOrDefaultAsync(p => p.Id == id);
                    if (existingProduct == null) return NotFound();

                    existingProduct.Name = product.Name;
                    existingProduct.Description = product.Description;
                    existingProduct.Price = product.Price;
                    existingProduct.CategoryId = product.CategoryId;
                    existingProduct.StockQuantity = product.StockQuantity;
                    existingProduct.Niche = product.Niche;
                    existingProduct.ImageUrl1 = product.ImageUrl1;
                    existingProduct.ImageUrl2 = product.ImageUrl2;
                    existingProduct.ImageUrl3 = product.ImageUrl3;
                    existingProduct.ImageUrl4 = product.ImageUrl4;
                    existingProduct.ImageUrl5 = product.ImageUrl5;

                    await _context.SaveChangesAsync();

                    if (specKeys != null && specValues != null)
                    {
                        _context.ProductSpecifications.RemoveRange(existingProduct.Specifications);
                        var specs = new List<ProductSpecification>();
                        for (int i = 0; i < specKeys.Length; i++)
                        {
                            if (!string.IsNullOrWhiteSpace(specKeys[i]) && !string.IsNullOrWhiteSpace(specValues[i]))
                            {
                                specs.Add(new ProductSpecification
                                {
                                    ProductId = product.Id,
                                    Key = specKeys[i].Trim(),
                                    Value = specValues[i].Trim()
                                });
                            }
                        }
                        if (specs.Any())
                        {
                            _context.ProductSpecifications.AddRange(specs);
                        }
                    }

                    if (colors != null)
                    {
                        _context.ProductColors.RemoveRange(existingProduct.Colors);
                        var productColors = new List<ProductColor>();
                        foreach (var color in colors)
                        {
                            if (!string.IsNullOrWhiteSpace(color))
                            {
                                productColors.Add(new ProductColor
                                {
                                    ProductId = product.Id,
                                    ColorName = color.Trim()
                                });
                            }
                        }
                        if (productColors.Any())
                        {
                            _context.ProductColors.AddRange(productColors);
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.CategoryId = new SelectList(_context.ShopCategories, "Id", "Name", product.CategoryId);
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