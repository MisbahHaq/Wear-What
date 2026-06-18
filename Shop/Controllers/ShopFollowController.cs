using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;

namespace Shop.Controllers
{
    [Authorize]
    public class ShopFollowController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ShopFollowController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Follow(int shopId, string? returnUrl = null)
        {
            var userId = GetCurrentUserId();
            var exists = await _context.ShopFollows.AnyAsync(f => f.UserId == userId && f.ShopId == shopId);
            
            if (!exists)
            {
                _context.ShopFollows.Add(new ShopFollow { UserId = userId, ShopId = shopId });
                await _context.SaveChangesAsync();
            }

            if (!string.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Shops");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unfollow(int shopId, string? returnUrl = null)
        {
            var userId = GetCurrentUserId();
            var follow = await _context.ShopFollows.FirstOrDefaultAsync(f => f.UserId == userId && f.ShopId == shopId);
            
            if (follow != null)
            {
                _context.ShopFollows.Remove(follow);
                await _context.SaveChangesAsync();
            }

            if (!string.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Shops");
        }
    }
}