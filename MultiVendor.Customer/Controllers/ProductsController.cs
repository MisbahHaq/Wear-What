using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MultiVendor.Core.Data;
using MultiVendor.Core.Models;

namespace MultiVendor.Customer.Controllers
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

        [AllowAnonymous]
        public async Task<IActionResult> Index(int? categoryId, string? searchString, decimal? minPrice, decimal? maxPrice, bool? inStockOnly, int? minRating)
        {
            var query = _context.Products.Include(p => p.Shop).Include(p => p.Category).AsQueryable();

            if (User.Identity?.IsAuthenticated == true && (User.IsInRole("Admin") || User.IsInRole("Vendor")))
            {
                var userId = GetCurrentUserId();
                var userShopIds = await _context.Shops
                    .Where(s => s.OwnerId == userId)
                    .Select(s => s.Id)
                    .ToListAsync();

                if (!User.IsInRole("Admin"))
                {
                    query = query.Where(p => p.ModerationStatus == "Approved" || userShopIds.Contains(p.ShopId));
                }
            }
            else
            {
                query = query.Where(p => p.ModerationStatus == "Approved");
            }

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);
            if (!string.IsNullOrWhiteSpace(searchString))
                query = query.Where(p => (p.Name != null && p.Name.Contains(searchString)) || (p.Description != null && p.Description.Contains(searchString)) || (p.Shop != null && p.Shop.Name.Contains(searchString)));
            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);
            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);
            if (inStockOnly == true)
                query = query.Where(p => p.StockQuantity > 0);
            if (minRating.HasValue)
            {
                var minR = minRating.Value;
                query = query.Where(p => p.Comments.Any(c => c.Rating >= minR));
            }

            var products = await query.OrderByDescending(p => p.Id).ToListAsync();
            ViewBag.CategoryId = new SelectList(_context.ShopCategories, "Id", "Name", categoryId);
            ViewBag.SearchString = searchString;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.InStockOnly = inStockOnly;
            ViewBag.MinRating = minRating;
            return View(products);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Suggest(string? term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(new List<object>());

            var matches = await _context.Products
                .Include(p => p.Shop)
                .Where(p => (p.Name != null && p.Name.Contains(term)) || (p.Description != null && p.Description.Contains(term)) || (p.Shop != null && p.Shop.Name.Contains(term)))
                .OrderBy(p => p.Name)
                .Take(8)
                .Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    image = p.ImageUrl1,
                    price = p.Price,
                    shop = p.Shop != null ? p.Shop.Name : ""
                })
                .ToListAsync();

            return Json(matches);
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

            var related = await _context.Products
                .Include(p => p.Shop)
                .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id && p.ModerationStatus == "Approved")
                .OrderByDescending(p => p.Id)
                .Take(6)
                .ToListAsync();

            ViewBag.RelatedProducts = related;

            return View(product);
        }
    }
}
