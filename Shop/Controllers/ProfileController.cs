using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;

namespace Shop.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public ProfileController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var isVendor = await _userManager.IsInRoleAsync(user, "Vendor");

            var viewModel = new ProfileViewModel
            {
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                Address = user.Address,
                Shops = await _context.Shops.Where(s => s.OwnerId == user.Id).ToListAsync(),
                WishlistItems = await _context.WishlistItems
                    .Include(w => w.Product)
                    .ThenInclude(p => p!.Shop)
                    .Where(w => w.UserId == user.Id)
                    .ToListAsync(),
                Orders = await _context.Orders
                    .Where(o => o.CustomerId == user.Id)
                    .OrderByDescending(o => o.CreatedAt)
                    .Select(o => new UserOrderSummary
                    {
                        OrderId = o.Id,
                        CreatedAt = o.CreatedAt,
                        Status = o.Status,
                        TotalAmount = o.TotalAmount,
                        ItemCount = o.OrderItems.Count
                    })
                    .ToListAsync()
            };

            ViewBag.IsVendor = isVendor;
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (ModelState.IsValid)
            {
                user.FullName = model.FullName;
                user.Address = model.Address;
                
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    ViewBag.Message = "Profile updated successfully.";
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            model.Email = user.Email ?? string.Empty;
            model.Shops = await _context.Shops.Where(s => s.OwnerId == user.Id).ToListAsync();
            model.WishlistItems = await _context.WishlistItems
                .Include(w => w.Product)
                .ThenInclude(p => p!.Shop)
                .Where(w => w.UserId == user.Id)
                .ToListAsync();
            model.Orders = await _context.Orders
                .Where(o => o.CustomerId == user.Id)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new UserOrderSummary
                {
                    OrderId = o.Id,
                    CreatedAt = o.CreatedAt,
                    Status = o.Status,
                    TotalAmount = o.TotalAmount,
                    ItemCount = o.OrderItems.Count
                })
                .ToListAsync();
            return View(model);
        }

        public async Task<IActionResult> ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return NotFound();

                var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    ViewBag.Message = "Password changed successfully.";
                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        public async Task<IActionResult> DeleteAccount()
        {
            return View();
        }

        [HttpPost, ActionName("DeleteAccount")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccountConfirmed()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                await _signInManager.SignOutAsync();
                return RedirectToAction("Index", "Home");
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View();
        }
    }
}