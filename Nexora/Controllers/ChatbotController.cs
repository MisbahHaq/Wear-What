using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexora.Data;
using Nexora.Models;

namespace Nexora.Controllers
{
    public class ChatbotController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ChatbotController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Status([FromBody] ChatbotOrderStatusRequest request)
        {
            if (!TryParseOrderNumber(request?.OrderNumber, out var orderId))
            {
                return Json(new { success = false, message = "Please enter a valid order number, for example 12." });
            }

            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return Json(new { success = false, message = "We could not find an order with that number." });
            }

            return Json(new
            {
                success = true,
                message = $"Order #{order.Id} is currently {order.Status}.",
                order = new
                {
                    order.Id,
                    order.Status,
                    order.OrderDate,
                    order.TotalAmount,
                    itemCount = order.Items.Count
                }
            });
        }

        [HttpPost]
        public async Task<IActionResult> Cancel([FromBody] ChatbotCancelOrderRequest request)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return LoginRequired("request cancellations");
            }

            if (!TryParseOrderNumber(request?.OrderNumber, out var orderId))
            {
                return Json(new { success = false, message = "Please enter a valid order number." });
            }

            if (string.IsNullOrWhiteSpace(request?.Reason))
            {
                return Json(new { success = false, message = "Please provide a cancellation reason." });
            }

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId && o.UserEmail == userEmail);
            if (order == null)
            {
                return Json(new { success = false, message = "We could not find an order with that number." });
            }

            if (order.Status == "Cancelled")
            {
                return Json(new { success = false, message = "This order is already cancelled." });
            }

            if (order.Status == "Delivered")
            {
                return Json(new { success = false, message = "Delivered orders cannot be cancelled through chatbot." });
            }

            var customerName = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrWhiteSpace(customerName))
            {
                customerName = userEmail;
            }

            var supportRequest = new SupportRequest
            {
                RequestType = "Cancellation",
                OrderId = order.Id,
                CustomerName = customerName,
                CustomerEmail = userEmail,
                Reason = request.Reason,
                Message = $"Cancellation requested for order #{order.Id}. Reason: {request.Reason}",
                Status = "New",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.SupportRequests.Add(supportRequest);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Cancellation request sent to admin. Admin will review it and cancel the order if eligible.",
                requestId = supportRequest.Id
            });
        }

        [HttpPost]
        public async Task<IActionResult> NotifyAdmin([FromBody] ChatbotAdminChatRequest request)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return LoginRequired("contact admin");
            }

            if (string.IsNullOrWhiteSpace(request?.Message))
            {
                return Json(new { success = false, message = "Please type a message for admin." });
            }

            int? orderId = null;
            if (!string.IsNullOrWhiteSpace(request.OrderNumber))
            {
                if (!TryParseOrderNumber(request.OrderNumber, out var parsedOrderId))
                {
                    return Json(new { success = false, message = "Please enter a valid order number." });
                }

                if (!await _context.Orders.AnyAsync(o => o.Id == parsedOrderId && o.UserEmail == userEmail))
                {
                    return Json(new { success = false, message = "We could not find an order with that number." });
                }

                orderId = parsedOrderId;
            }

            var customerName = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrWhiteSpace(customerName))
            {
                customerName = userEmail;
            }

            var supportRequest = new SupportRequest
            {
                RequestType = "AdminChat",
                OrderId = orderId,
                CustomerName = customerName,
                CustomerEmail = userEmail,
                Reason = string.Empty,
                Message = request.Message,
                Status = "New",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.SupportRequests.Add(supportRequest);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Admin has been notified. They will respond from the admin panel.",
                requestId = supportRequest.Id
            });
        }

        [HttpGet]
        public async Task<IActionResult> Replies()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return StatusCode(401, new
                {
                    success = false,
                    message = "Please log in to view admin replies.",
                    redirectUrl = Url.Action("Login", "Account")
                });
            }

            var replies = await _context.SupportRequests
                .Where(r => r.CustomerEmail == userEmail && !string.IsNullOrWhiteSpace(r.AdminResponse))
                .OrderBy(r => r.CreatedAt)
                .Select(r => new
                {
                    id = r.Id,
                    requestType = r.RequestType,
                    status = r.Status,
                    message = r.Message,
                    adminResponse = r.AdminResponse,
                    createdAt = r.CreatedAt,
                    updatedAt = r.UpdatedAt
                })
                .ToListAsync();

            return Json(new { success = true, replies });
        }

        private IActionResult LoginRequired(string action)
        {
            return StatusCode(401, new
            {
                success = false,
                message = $"Please log in to {action}.",
                redirectUrl = Url.Action("Login", "Account")
            });
        }

        private static bool TryParseOrderNumber(string? value, out int orderId)
        {
            orderId = 0;
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var normalized = value.Trim();
            if (normalized.StartsWith("#", StringComparison.Ordinal))
            {
                normalized = normalized[1..];
            }

            return int.TryParse(normalized, out orderId);
        }
    }

    public class ChatbotOrderStatusRequest
    {
        public string? OrderNumber { get; set; }
    }

    public class ChatbotCancelOrderRequest
    {
        public string? OrderNumber { get; set; }
        public string? Reason { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
    }

    public class ChatbotAdminChatRequest
    {
        public string? OrderNumber { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public string? Message { get; set; }
    }
}
