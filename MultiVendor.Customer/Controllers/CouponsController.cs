using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiVendor.Core.Data;
using MultiVendor.Core.Models;

namespace MultiVendor.Customer.Controllers
{
    public class CouponsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CouponsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Validate(string code, decimal orderTotal)
        {
            if (string.IsNullOrWhiteSpace(code))
                return Json(new { valid = false, error = "Please enter a coupon code." });

            var coupon = await _context.Coupons
                .FirstOrDefaultAsync(c => c.Code == code && c.IsActive);

            if (coupon == null)
                return Json(new { valid = false, error = "Invalid coupon code." });

            if (coupon.ExpiryDate.HasValue && coupon.ExpiryDate < DateTime.UtcNow)
                return Json(new { valid = false, error = "Coupon has expired." });

            if (coupon.UsageLimit > 0 && coupon.UsedCount >= coupon.UsageLimit)
                return Json(new { valid = false, error = "Coupon usage limit reached." });

            if (coupon.MinOrderAmount.HasValue && orderTotal < coupon.MinOrderAmount)
                return Json(new { valid = false, error = $"Minimum order amount is ${coupon.MinOrderAmount.Value}." });

            var discount = coupon.DiscountType == "Percentage"
                ? orderTotal * (coupon.DiscountValue / 100m)
                : coupon.DiscountValue;

            return Json(new { valid = true, discount = Math.Min(discount, orderTotal) });
        }
    }
}

