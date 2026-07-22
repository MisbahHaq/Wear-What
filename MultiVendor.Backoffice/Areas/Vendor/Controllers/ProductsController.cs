using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MultiVendor.Core.Data;
using MultiVendor.Core.Models;
using System.Globalization;
using System.Text;

namespace MultiVendor.Backoffice.Areas.Vendor.Controllers
{
    [Authorize(Roles = "Vendor")]
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

                product.ModerationStatus = "Pending";
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

                return RedirectToAction("Index", "Home");
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
                    existingProduct.LowStockThreshold = product.LowStockThreshold;
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
                return RedirectToAction("Index", "Home");
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
            return RedirectToAction("Index", "Home");
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }

        public async Task<IActionResult> ExportCsv()
        {
            var currentUserId = GetCurrentUserId();
            var shopIds = await _context.Shops
                .Where(s => s.OwnerId == currentUserId)
                .Select(s => s.Id)
                .ToListAsync();

            var products = await _context.Products
                .Where(p => shopIds.Contains(p.ShopId))
                .Include(p => p.Category)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Id,Name,Description,Price,StockQuantity,LowStockThreshold,CategoryId,CategoryName,ShopId");

            foreach (var p in products)
            {
                sb.AppendLine($"{p.Id},\"{p.Name}\",\"{p.Description}\",{p.Price},{p.StockQuantity},{p.LowStockThreshold},{p.CategoryId},{p.Category?.Name},{p.ShopId}");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "products.csv");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportCsv(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ProductMessage"] = "Please select a CSV file.";
                return RedirectToAction("Index", "Home");
            }

            var currentUserId = GetCurrentUserId();
            var shop = await _context.Shops.FirstOrDefaultAsync(s => s.OwnerId == currentUserId);
            if (shop == null)
            {
                TempData["ProductMessage"] = "You must have a shop to import products.";
                return RedirectToAction("Index", "Home");
            }

            using var reader = new StreamReader(file.OpenReadStream());
            var header = await reader.ReadLineAsync();
            var imported = 0;
            var errors = new List<string>();

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;

                try
                {
                    var parts = line.Split(',');
                    if (parts.Length < 5) continue;

                    var product = new Product
                    {
                        Name = parts[1].Trim('"'),
                        Description = parts[2].Trim('"'),
                        Price = decimal.Parse(parts[3], CultureInfo.InvariantCulture),
                        StockQuantity = int.Parse(parts[4]),
                        LowStockThreshold = parts.Length > 5 && int.TryParse(parts[5], out var l) ? l : 5,
                        ShopId = shop.Id,
                        ModerationStatus = "Pending"
                    };

                    if (parts.Length > 6 && int.TryParse(parts[6], out var catId))
                        product.CategoryId = catId;

                    _context.Products.Add(product);
                    imported++;
                }
                catch
                {
                    errors.Add(line);
                }
            }

            await _context.SaveChangesAsync();
            TempData["ProductMessage"] = $"Imported {imported} products.{(errors.Any() ? $" Skipped {errors.Count} invalid rows." : "")}";
            return RedirectToAction("Index", "Home");
        }
    }
}
