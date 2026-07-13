using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;

namespace Shop.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private string GetCurrentUserId() => _userManager.GetUserId(User) ?? string.Empty;

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

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int productId, int quantity, string? shippingAddress = null, string? contactNumber = null, string? deliveryMethod = null, string? paymentMethod = null, string? returnUrl = null)
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

            var shipping = !string.IsNullOrWhiteSpace(shippingAddress) ? shippingAddress : user.Address;

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
                Status = "Pending",
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

            order.TotalAmount = product.Price * quantity + delivery;
            product.StockQuantity -= quantity;
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

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

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCart(string? shippingAddress = null, string? contactNumber = null, string? deliveryMethod = null, string? paymentMethod = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var cart = GetCartFromSession();
            if (!cart.Any())
            {
                return RedirectToAction(nameof(Index), "Cart");
            }

            var shipping = !string.IsNullOrWhiteSpace(shippingAddress) ? shippingAddress : user.Address;

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

            foreach (var group in grouped)
            {
                var order = new Order
                {
                    CustomerId = user.Id,
                    CreatedAt = DateTime.UtcNow,
                    Status = "Pending",
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
                        ProductId = product.Id,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price
                    });
                    product.StockQuantity -= item.Quantity;
                    total += product.Price * item.Quantity;
                }

                order.TotalAmount = total + delivery;
                _context.Orders.Add(order);
                lastOrder = order;
            }

            await _context.SaveChangesAsync();

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

        public async Task<IActionResult> Dashboard()
        {
            var currentUserId = GetCurrentUserId();
            var shopIds = await _context.Shops
                .Where(s => s.OwnerId == currentUserId)
                .Select(s => s.Id)
                .ToListAsync();

            var orderItems = await _context.OrderItems
                .Include(i => i.Order)
                .ThenInclude(o => o!.Customer)
                .Include(i => i.Product)
                .ThenInclude(p => p!.Shop)
                .Where(i => shopIds.Contains(i.Product!.ShopId))
                .OrderByDescending(i => i.Order!.CreatedAt)
                .ToListAsync();

            var shops = await _context.Shops
                .Where(s => shopIds.Contains(s.Id))
                .ToListAsync();

            var viewModel = new OwnerDashboardViewModel
            {
                Shops = shops,
                OrderItems = orderItems,
                TotalSales = orderItems.Sum(i => i.UnitPrice * i.Quantity)
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int orderItemId, string status)
        {
            var currentUserId = GetCurrentUserId();
            var shopIds = await _context.Shops
                .Where(s => s.OwnerId == currentUserId)
                .Select(s => s.Id)
                .ToListAsync();

            var orderItem = await _context.OrderItems
                .Include(i => i.Product)
                .Include(i => i.Order)
                .FirstOrDefaultAsync(i => i.Id == orderItemId);

            if (orderItem == null || orderItem.Product == null || !shopIds.Contains(orderItem.Product.ShopId))
                return Forbid();

            if (orderItem.Order != null)
            {
                orderItem.Order.Status = status;
                await _context.SaveChangesAsync();
                TempData["StatusMessage"] = "Order status updated.";
            }

            return RedirectToAction(nameof(Dashboard));
        }
    }
}
