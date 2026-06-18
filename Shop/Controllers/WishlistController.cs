using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;

namespace Shop.Controllers
{
    [Authorize]
    public class WishlistController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public WishlistController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(int? productId)
        {
            var query = _context.WishlistItems
                .Include(w => w.Product)
                .ThenInclude(p => p!.Shop)
                .AsQueryable();

            if (productId.HasValue)
            {
                query = query.Where(w => w.ProductId == productId.Value);
            }

            var items = await query.ToListAsync();
            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int productId, string? returnUrl = null)
        {
            var userId = GetCurrentUserId();
            var exists = await _context.WishlistItems.AnyAsync(w => w.UserId == userId && w.ProductId == productId);
            
            if (!exists)
            {
                _context.WishlistItems.Add(new WishlistItem { UserId = userId, ProductId = productId });
                await _context.SaveChangesAsync();
            }

            if (!string.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Products");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int productId, string? returnUrl = null)
        {
            var userId = GetCurrentUserId();
            var item = await _context.WishlistItems.FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);
            
            if (item != null)
            {
                _context.WishlistItems.Remove(item);
                await _context.SaveChangesAsync();
            }

            if (!string.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Products");
        }
    }
}