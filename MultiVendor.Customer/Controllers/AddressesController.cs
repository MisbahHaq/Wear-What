using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MultiVendor.Core.Data;
using MultiVendor.Core.Models;

namespace MultiVendor.Customer.Controllers
{
    [Authorize]
    public class AddressesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AddressesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private string GetCurrentUserId() => _userManager.GetUserId(User) ?? string.Empty;

        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var addresses = await _context.UserAddresses
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.IsDefault)
                .ToListAsync();
            return View(addresses);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserAddress address)
        {
            var userId = GetCurrentUserId();
            address.UserId = userId;
            address.CreatedAt = DateTime.UtcNow;

            if (address.IsDefault)
            {
                var others = await _context.UserAddresses
                    .Where(a => a.UserId == userId && a.IsDefault)
                    .ToListAsync();
                foreach (var a in others) a.IsDefault = false;
            }

            _context.UserAddresses.Add(address);
            await _context.SaveChangesAsync();

            TempData["AddressMessage"] = "Address added.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var userId = GetCurrentUserId();
            var address = await _context.UserAddresses
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
            if (address == null) return NotFound();
            return View(address);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserAddress address)
        {
            if (id != address.Id) return NotFound();

            var userId = GetCurrentUserId();
            var existing = await _context.UserAddresses
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
            if (existing == null) return NotFound();

            existing.AddressLine = address.AddressLine;
            existing.City = address.City;
            existing.State = address.State;
            existing.ZipCode = address.ZipCode;
            existing.Country = address.Country;

            if (address.IsDefault)
            {
                var others = await _context.UserAddresses
                    .Where(a => a.UserId == userId && a.Id != id && a.IsDefault)
                    .ToListAsync();
                foreach (var a in others) a.IsDefault = false;
                existing.IsDefault = true;
            }

            await _context.SaveChangesAsync();
            TempData["AddressMessage"] = "Address updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();
            var address = await _context.UserAddresses
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
            if (address != null)
            {
                _context.UserAddresses.Remove(address);
                await _context.SaveChangesAsync();
            }
            TempData["AddressMessage"] = "Address deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}

