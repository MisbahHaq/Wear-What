using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;

namespace Shop.Controllers
{
    [Authorize]
    public class RatingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RatingsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private string GetCurrentUserId() => _userManager.GetUserId(User) ?? string.Empty;

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int productId, int rating)
        {
            if (rating < 1 || rating > 5)
            {
                TempData["RatingMessage"] = "Rating must be between 1 and 5.";
                return RedirectToAction("Details", "Products", new { id = productId });
            }

            var userId = GetCurrentUserId();
            var isVerified = await _context.Orders
                .AnyAsync(o => o.CustomerId == userId && o.Status == "Delivered" && o.OrderItems.Any(i => i.ProductId == productId));

            var existing = await _context.ProductRatings
                .FirstOrDefaultAsync(r => r.ProductId == productId && r.UserId == userId);

            if (existing != null)
            {
                existing.Rating = rating;
                existing.IsVerifiedPurchase = isVerified;
                existing.CreatedAt = DateTime.UtcNow;
            }
            else
            {
                _context.ProductRatings.Add(new ProductRating
                {
                    ProductId = productId,
                    UserId = userId,
                    Rating = rating,
                    IsVerifiedPurchase = isVerified
                });
            }

            await _context.SaveChangesAsync();
            TempData["RatingMessage"] = "Rating saved.";
            return RedirectToAction("Details", "Products", new { id = productId });
        }
    }
}
