using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MultiVendor.Core.Data;
using MultiVendor.Core.Models;
using MultiVendor.Customer.Models;

namespace MultiVendor.Customer.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private const int ReturnWindowDays = 14;

        public OrdersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private string GetCurrentUserId() => _userManager.GetUserId(User) ?? string.Empty;

        private async Task AddStatusHistoryAsync(int orderId, string status, string? note = null)
        {
            var userId = GetCurrentUserId();
            _context.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderId = orderId,
                Status = status,
                ChangedByUserId = userId,
                Timestamp = DateTime.UtcNow,
                Note = note
            });
            await _context.SaveChangesAsync();
        }

        private bool CanCancel(string status) => status == "Placed" || status == "Confirmed";

        private async Task SendNotificationAsync(string userId, string message, string? link = null)
        {
            _context.Notifications.Add(new Notification
            {
                UserId = userId,
                Message = message,
                Link = link,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        private async Task<(decimal Discount, string? Error)> ValidateCouponAsync(string? code, decimal orderTotal)
        {
            if (string.IsNullOrWhiteSpace(code))
                return (0, null);

            var coupon = await _context.Coupons
                .FirstOrDefaultAsync(c => c.Code == code && c.IsActive);

            if (coupon == null)
                return (0, "Invalid coupon code.");

            if (coupon.ExpiryDate.HasValue && coupon.ExpiryDate < DateTime.UtcNow)
                return (0, "Coupon has expired.");

            if (coupon.UsageLimit > 0 && coupon.UsedCount >= coupon.UsageLimit)
                return (0, "Coupon usage limit reached.");

            if (coupon.MinOrderAmount.HasValue && orderTotal < coupon.MinOrderAmount)
                return (0, $"Minimum order amount is ${coupon.MinOrderAmount.Value}.");

            var discount = coupon.DiscountType == "Percentage"
                ? orderTotal * (coupon.DiscountValue / 100m)
                : coupon.DiscountValue;

            return (Math.Min(discount, orderTotal), null);
        }

        private async Task RecordCommissionAsync(Order order)
        {
            var commissionRate = 0.10m;
            foreach (var item in order.OrderItems)
            {
                if (item.Product == null || item.Product.Shop == null) continue;

                var commission = item.UnitPrice * item.Quantity * commissionRate;
                var net = item.UnitPrice * item.Quantity - commission;

                _context.SellerTransactions.Add(new SellerTransaction
                {
                    SellerId = item.Product.Shop.OwnerId,
                    OrderId = order.Id,
                    CommissionRate = commissionRate,
                    CommissionAmount = commission,
                    NetAmount = net,
                    CreatedAt = DateTime.UtcNow
                });
            }
            await _context.SaveChangesAsync();
        }

        [HttpGet]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> Checkout(int productId, int quantity = 1)
        {
            if (quantity <= 0) quantity = 1;

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var product = await _context.Products
                .Include(p => p.Shop)
                .FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null) return NotFound();

            if (product.Shop?.OwnerId == user.Id)
            {
                TempData["OrderMessage"] = "You cannot purchase your own product.";
                return RedirectToAction("Details", "Products", new { id = productId });
            }

            var viewModel = new CheckoutViewModel
            {
                ProductId = product.Id,
                ProductName = product.Name,
                ProductImageUrl = product.ImageUrl1,
                ShopName = product.Shop?.Name,
                UnitPrice = product.Price,
                Quantity = quantity,
                CustomerFullName = user.FullName,
                CustomerContactNumber = user.ContactNumber ?? string.Empty,
                ShippingAddress = user.Address,
                DeliveryMethod = "Standard",
                PaymentMethod = "CashOnDelivery"
            };

            ViewBag.UserAddresses = await _context.UserAddresses
                .Where(a => a.UserId == user.Id)
                .ToListAsync();

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int productId, int quantity, string? shippingAddress = null, string? contactNumber = null, string? deliveryMethod = null, string? paymentMethod = null, string? returnUrl = null, int? selectedAddressId = null, string? couponCode = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Shop)
                .FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null) return NotFound();

            if (quantity <= 0)
            {
                ModelState.AddModelError(nameof(quantity), "Quantity must be greater than zero.");
            }

            if (product.StockQuantity < quantity)
            {
                ModelState.AddModelError(nameof(quantity), $"Only {product.StockQuantity} item(s) available in stock.");
            }

            if (!ModelState.IsValid)
            {
                return RedirectToAction("Details", "Products", new { id = productId });
            }

            string shipping;
            if (selectedAddressId.HasValue)
            {
                var address = await _context.UserAddresses
                    .FirstOrDefaultAsync(a => a.Id == selectedAddressId.Value && a.UserId == user.Id);
                if (address != null)
                {
                    shipping = $"{address.AddressLine}, {address.City}, {address.State} {address.ZipCode}, {address.Country}";
                }
                else
                {
                    shipping = !string.IsNullOrWhiteSpace(shippingAddress) ? shippingAddress : user.Address;
                }
            }
            else
            {
                shipping = !string.IsNullOrWhiteSpace(shippingAddress) ? shippingAddress : user.Address;
            }

            if (!string.IsNullOrWhiteSpace(contactNumber) && contactNumber != user.ContactNumber)
            {
                user.ContactNumber = contactNumber;
                await _userManager.UpdateAsync(user);
            }

            var delivery = deliveryMethod == "Express" ? CheckoutViewModel.ExpressDeliveryFee : 0m;
            var payment = paymentMethod == "Card" ? "Card" : "CashOnDelivery";

            var order = new Order
            {
                CustomerId = user.Id,
                CreatedAt = DateTime.UtcNow,
                Status = "Placed",
                ShippingAddress = shipping,
                DeliveryMethod = deliveryMethod == "Express" ? "Express" : "Standard",
                DeliveryFee = delivery,
                PaymentMethod = payment
            };

            order.OrderItems.Add(new OrderItem
            {
                ProductId = product.Id,
                Quantity = quantity,
                UnitPrice = product.Price
            });

            var subtotal = product.Price * quantity;
            var (discount, couponError) = await ValidateCouponAsync(couponCode, subtotal + delivery);
            if (couponError != null)
            {
                TempData["OrderMessage"] = couponError;
                return RedirectToAction("Details", "Products", new { id = productId });
            }

            order.TotalAmount = subtotal + delivery - discount;
            product.StockQuantity -= quantity;
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            await AddStatusHistoryAsync(order.Id, "Placed", "Order placed");
            await RecordCommissionAsync(order);

            if (!string.IsNullOrWhiteSpace(couponCode) && discount > 0)
            {
                var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == couponCode && c.IsActive);
                if (coupon != null)
                {
                    coupon.UsedCount++;
                    await _context.SaveChangesAsync();
                }
            }

            if (product.Shop?.OwnerId != null)
            {
                await SendNotificationAsync(product.Shop.OwnerId, $"New order #{order.Id} received for {product.Name}", $"/Orders/Dashboard");
            }

            HttpContext.Session.SetString("LastOrderId", order.Id.ToString());
            TempData["OrderMessage"] = "Order placed successfully.";

            return RedirectToAction(nameof(OrderConfirmation));
        }

        [HttpGet]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> CheckoutCart()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var cart = GetCartFromSession();
            if (!cart.Any())
            {
                return RedirectToAction(nameof(Index), "Cart");
            }

            var viewModel = new CartCheckoutViewModel
            {
                Items = cart.Select(i => new CartCheckoutItemViewModel
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    ImageUrl = i.ImageUrl,
                    ShopName = i.ShopName,
                    UnitPrice = i.Price,
                    Quantity = i.Quantity
                }).ToList(),
                CustomerFullName = user.FullName,
                CustomerContactNumber = user.ContactNumber ?? string.Empty,
                ShippingAddress = user.Address,
                DeliveryMethod = "Standard",
                PaymentMethod = "CashOnDelivery"
            };

            ViewBag.UserAddresses = await _context.UserAddresses
                .Where(a => a.UserId == user.Id)
                .ToListAsync();

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCart(string? shippingAddress = null, string? contactNumber = null, string? deliveryMethod = null, string? paymentMethod = null, int? selectedAddressId = null, string? couponCode = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var cart = GetCartFromSession();
            if (!cart.Any())
            {
                return RedirectToAction(nameof(Index), "Cart");
            }

            string shipping;
            if (selectedAddressId.HasValue)
            {
                var address = await _context.UserAddresses
                    .FirstOrDefaultAsync(a => a.Id == selectedAddressId.Value && a.UserId == user.Id);
                if (address != null)
                {
                    shipping = $"{address.AddressLine}, {address.City}, {address.State} {address.ZipCode}, {address.Country}";
                }
                else
                {
                    shipping = !string.IsNullOrWhiteSpace(shippingAddress) ? shippingAddress : user.Address;
                }
            }
            else
            {
                shipping = !string.IsNullOrWhiteSpace(shippingAddress) ? shippingAddress : user.Address;
            }

            if (!string.IsNullOrWhiteSpace(contactNumber) && contactNumber != user.ContactNumber)
            {
                user.ContactNumber = contactNumber;
                await _userManager.UpdateAsync(user);
            }

            var delivery = deliveryMethod == "Express" ? CartCheckoutViewModel.ExpressDeliveryFee : 0m;
            var payment = paymentMethod == "Card" ? "Card" : "CashOnDelivery";
            Order? lastOrder = null;

            var productIds = cart.Select(i => i.ProductId).ToList();
            var products = await _context.Products
                .Include(p => p.Shop)
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            var stockErrors = new List<string>();
            foreach (var item in cart)
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product == null) continue;
                if (product.StockQuantity < item.Quantity)
                {
                    stockErrors.Add($"{product.Name}: only {product.StockQuantity} in stock.");
                }
            }
            if (stockErrors.Any())
            {
                TempData["CartMessage"] = "Some items are no longer available in the requested quantity: " + string.Join(" ", stockErrors);
                return RedirectToAction(nameof(Index), "Cart");
            }

            var grouped = cart
                .GroupBy(i => i.ShopId)
                .ToList();

            var createdOrders = new List<Order>();

            foreach (var group in grouped)
            {
                var order = new Order
                {
                    CustomerId = user.Id,
                    CreatedAt = DateTime.UtcNow,
                    Status = "Placed",
                    ShippingAddress = shipping,
                    DeliveryMethod = deliveryMethod == "Express" ? "Express" : "Standard",
                    DeliveryFee = delivery,
                    PaymentMethod = payment
                };

                decimal total = 0;
                foreach (var item in group)
                {
                    var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                    if (product == null) continue;

                    order.OrderItems.Add(new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price
                    });
                    product.StockQuantity -= item.Quantity;
                    total += product.Price * item.Quantity;
                }

                var orderSubtotal = total + delivery;
                var (discount, couponError) = await ValidateCouponAsync(couponCode, orderSubtotal);
                if (couponError != null)
                {
                    TempData["CartMessage"] = couponError;
                    return RedirectToAction(nameof(Index), "Cart");
                }

                order.TotalAmount = orderSubtotal - discount;
                _context.Orders.Add(order);
                createdOrders.Add(order);
                lastOrder = order;
            }

            await _context.SaveChangesAsync();

            foreach (var o in createdOrders)
            {
                await AddStatusHistoryAsync(o.Id, "Placed", "Order placed");
                await RecordCommissionAsync(o);

                var firstItem = o.OrderItems.FirstOrDefault();
                if (firstItem?.Product?.Shop?.OwnerId != null)
                {
                    await SendNotificationAsync(firstItem.Product.Shop.OwnerId, $"New order #{o.Id} received", $"/Orders/Dashboard");
                }
            }

            if (!string.IsNullOrWhiteSpace(couponCode))
            {
                var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == couponCode && c.IsActive);
                if (coupon != null && createdOrders.Any())
                {
                    coupon.UsedCount++;
                    await _context.SaveChangesAsync();
                }
            }

            if (lastOrder != null)
            {
                HttpContext.Session.SetString("LastOrderId", lastOrder.Id.ToString());
            }

            ClearCartSession();
            TempData["OrderMessage"] = "Your order has been placed successfully.";

            return RedirectToAction(nameof(OrderConfirmation));
        }

        [HttpGet]
        public async Task<IActionResult> OrderConfirmation()
        {
            var idStr = HttpContext.Session.GetString("LastOrderId");
            if (!int.TryParse(idStr, out var orderId))
            {
                return RedirectToAction("Index", "Home");
            }

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.CustomerId == GetCurrentUserId());

            if (order == null) return RedirectToAction("Index", "Home");

            return View(order);
        }

        private List<CartItem> GetCartFromSession()
        {
            var cartJson = HttpContext.Session.GetString(CartSessionKey);
            return string.IsNullOrEmpty(cartJson) ? new List<CartItem>() : System.Text.Json.JsonSerializer.Deserialize<List<CartItem>>(cartJson)!;
        }

        private void ClearCartSession()
        {
            HttpContext.Session.Remove(CartSessionKey);
        }

        private const string CartSessionKey = "Cart";

        [HttpGet]
        public async Task<IActionResult> Tracking(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(i => i.Product)
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            var currentUserId = GetCurrentUserId();
            if (order.CustomerId != currentUserId && !User.IsInRole("Admin"))
            {
                var shopIds = await _context.Shops
                    .Where(s => s.OwnerId == currentUserId)
                    .Select(s => s.Id)
                    .ToListAsync();

                var isSeller = order.OrderItems.Any(i => i.Product != null && shopIds.Contains(i.Product.ShopId));
                if (!isSeller) return Forbid();
            }

            var history = await _context.OrderStatusHistories
                .Where(h => h.OrderId == id)
                .OrderByDescending(h => h.Timestamp)
                .ToListAsync();

            return View(new OrderTrackingViewModel
            {
                Order = order,
                StatusHistory = history
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, string? reason = null)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            var currentUserId = GetCurrentUserId();
            var isBuyer = order.CustomerId == currentUserId;
            var isSeller = false;

            if (!isBuyer && !User.IsInRole("Admin"))
            {
                var shopIds = await _context.Shops
                    .Where(s => s.OwnerId == currentUserId)
                    .Select(s => s.Id)
                    .ToListAsync();
                isSeller = order.OrderItems.Any(i => i.Product != null && shopIds.Contains(i.Product.ShopId));
                if (!isSeller) return Forbid();
            }

            if (!CanCancel(order.Status))
            {
                TempData["OrderMessage"] = "This order cannot be cancelled anymore.";
                return RedirectToAction(nameof(Tracking), new { id });
            }

            if (!isBuyer && string.IsNullOrWhiteSpace(reason))
            {
                TempData["OrderMessage"] = "Cancellation reason is required.";
                return RedirectToAction(nameof(Tracking), new { id });
            }

            order.Status = "Cancelled";
            foreach (var item in order.OrderItems)
            {
                if (item.Product != null)
                {
                    item.Product.StockQuantity += item.Quantity;
                }
            }

            await _context.SaveChangesAsync();
            await AddStatusHistoryAsync(order.Id, "Cancelled", reason ?? "Cancelled by buyer");

            TempData["OrderMessage"] = "Order cancelled successfully.";
            return RedirectToAction(nameof(Tracking), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestReturn(int orderItemId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["OrderMessage"] = "Please provide a reason for the return.";
                return RedirectToAction("Tracking", new { id = 0 });
            }

            var orderItem = await _context.OrderItems
                .Include(i => i.Product)
                .Include(i => i.Order)
                .FirstOrDefaultAsync(i => i.Id == orderItemId);

            if (orderItem == null || orderItem.Order == null || orderItem.Product == null)
                return NotFound();

            var currentUserId = GetCurrentUserId();
            if (orderItem.Order.CustomerId != currentUserId)
                return Forbid();

            if (orderItem.Order.Status != "Delivered")
            {
                TempData["OrderMessage"] = "Returns can only be requested for delivered orders.";
                return RedirectToAction("Tracking", new { id = orderItem.OrderId });
            }

            if (orderItem.Order.DeliveredAt == null || (DateTime.UtcNow - orderItem.Order.DeliveredAt.Value).TotalDays > ReturnWindowDays)
            {
                TempData["OrderMessage"] = $"Return window of {ReturnWindowDays} days has expired.";
                return RedirectToAction("Tracking", new { id = orderItem.OrderId });
            }

            var existing = await _context.ReturnRequests
                .AnyAsync(r => r.OrderItemId == orderItemId && r.Status != "Rejected" && r.RefundStatus != "Refunded");

            if (existing)
            {
                TempData["OrderMessage"] = "A return request already exists for this item.";
                return RedirectToAction("Tracking", new { id = orderItem.OrderId });
            }

            var sellerId = orderItem.Product.Shop?.OwnerId;
            var returnRequest = new ReturnRequest
            {
                OrderItemId = orderItemId,
                BuyerId = currentUserId,
                SellerId = sellerId,
                Reason = reason,
                Status = "Pending",
                RefundStatus = "Pending",
                RequestedAt = DateTime.UtcNow
            };

            _context.ReturnRequests.Add(returnRequest);

            await _context.SaveChangesAsync();
            await AddStatusHistoryAsync(orderItem.OrderId, "ReturnRequested", "Return requested by buyer");

            if (!string.IsNullOrEmpty(returnRequest.SellerId))
            {
                await SendNotificationAsync(returnRequest.SellerId, $"Return requested for order #{returnRequest.OrderItem.OrderId}", $"/Orders/Dashboard");
            }

            TempData["OrderMessage"] = "Return request submitted.";
            return RedirectToAction("Tracking", new { id = orderItem.OrderId });
        }
    }
}
