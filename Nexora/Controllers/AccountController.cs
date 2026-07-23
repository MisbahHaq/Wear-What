using Microsoft.AspNetCore.Mvc;
using Nexora.Data;
using Nexora.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;

namespace Nexora.Controllers;

[Route("api/[controller]")]
public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;

    public AccountController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        if (request.Email == "admin@represent.com" && request.Password == "Qwerty123")
        {
            HttpContext.Session.SetString("UserEmail", request.Email);
            HttpContext.Session.SetString("AuthToken", "fake-jwt-token");
            HttpContext.Session.SetString("UserName", "Admin");
            HttpContext.Session.SetString("UserAddress", string.Empty);
            HttpContext.Session.SetString("IsAdmin", "true");
            return Ok(new { success = true, user = new { email = request.Email, name = "Admin", isAdmin = true } });
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email && u.Password == request.Password);

        if (user == null)
        {
            return BadRequest(new { success = false, message = "Invalid email or password." });
        }

        var dbCartItems = await _context.ShoppingCarts.Where(c => c.UserEmail == request.Email).ToListAsync();
        var sessionCart = dbCartItems.Select(c => new CartItem { ProductId = c.ProductId, Quantity = c.Quantity, Size = c.Size ?? string.Empty, Color = c.Color ?? string.Empty }).ToList();
        var cartJson = JsonSerializer.Serialize(sessionCart);
        HttpContext.Session.SetString("Cart", cartJson);

        HttpContext.Session.SetString("UserEmail", request.Email);
        HttpContext.Session.SetString("AuthToken", "fake-jwt-token");
        HttpContext.Session.SetString("UserName", user.Name);
        HttpContext.Session.SetString("UserAddress", user.Address);

        return Ok(new { success = true, user = new { email = user.Email, name = user.Name, address = user.Address, isAdmin = false } });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (existingUser != null)
        {
            return BadRequest(new { success = false, message = "An account with this email already exists." });
        }

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            DateOfBirth = DateTime.SpecifyKind(request.DateOfBirth, DateTimeKind.Utc),
            Address = request.Address,
            Password = request.Password
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        HttpContext.Session.SetString("UserEmail", user.Email);
        HttpContext.Session.SetString("AuthToken", "fake-jwt-token");
        HttpContext.Session.SetString("UserName", user.Name);

        return Ok(new { success = true, user = new { email = user.Email, name = user.Name, address = user.Address } });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userEmail = HttpContext.Session.GetString("UserEmail");
        if (!string.IsNullOrEmpty(userEmail))
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            if (!string.IsNullOrEmpty(cartJson))
            {
                var cart = JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();

                foreach (var item in cart)
                {
                    var dbItem = item.Size == string.Empty && item.Color == string.Empty
                        ? await _context.ShoppingCarts.FirstOrDefaultAsync(c => c.UserEmail == userEmail && c.ProductId == item.ProductId && (c.Size == null || c.Size == item.Size) && (c.Color == null || c.Color == item.Color))
                        : await _context.ShoppingCarts.FirstOrDefaultAsync(c => c.UserEmail == userEmail && c.ProductId == item.ProductId && c.Size == item.Size && c.Color == item.Color);

                    if (dbItem != null)
                    {
                        dbItem.Quantity = item.Quantity;
                    }
                    else
                    {
                        _context.ShoppingCarts.Add(new ShoppingCart
                        {
                            UserEmail = userEmail,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            Size = item.Size,
                            Color = item.Color,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
                await _context.SaveChangesAsync();
            }
        }

        HttpContext.Session.Clear();
        return Ok(new { success = true });
    }

    [HttpGet("profile")]
    public async Task<IActionResult> Profile()
    {
        var userEmail = HttpContext.Session.GetString("UserEmail");
        if (string.IsNullOrEmpty(userEmail))
        {
            return Unauthorized(new { message = "Not logged in." });
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        var isAdmin = HttpContext.Session.GetString("IsAdmin") == "true";

        return Ok(new UserDto
        {
            Email = user.Email,
            Name = user.Name,
            Address = user.Address,
            DateOfBirth = user.DateOfBirth
        });
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto request)
    {
        var userEmail = HttpContext.Session.GetString("UserEmail");
        if (string.IsNullOrEmpty(userEmail))
        {
            return Unauthorized(new { message = "Not logged in." });
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        if (request.Name != null)
        {
            user.Name = request.Name;
            HttpContext.Session.SetString("UserName", request.Name);
        }

        if (request.Address != null)
        {
            user.Address = request.Address;
            HttpContext.Session.SetString("UserAddress", request.Address);
        }

        await _context.SaveChangesAsync();

        return Ok(new UserDto
        {
            Email = user.Email,
            Name = user.Name,
            Address = user.Address,
            DateOfBirth = user.DateOfBirth
        });
    }

    [HttpGet("check-auth")]
    public IActionResult CheckAuth()
    {
        var userEmail = HttpContext.Session.GetString("UserEmail");
        if (string.IsNullOrEmpty(userEmail))
        {
            return Ok(new { authenticated = false });
        }
        var isAdmin = HttpContext.Session.GetString("IsAdmin") == "true";
        return Ok(new { authenticated = true, userEmail = userEmail, userName = HttpContext.Session.GetString("UserName") ?? string.Empty, isAdmin = isAdmin });
    }

    [HttpGet("~/Account/Login")]
    public IActionResult LoginPage(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View("Login");
    }

    [HttpGet("~/Account/SignUp")]
    public IActionResult SignUpPage()
    {
        return View("SignUp");
    }

    [HttpGet("~/Account/Profile")]
    public IActionResult ProfilePage()
    {
        return View("Profile");
    }

    [HttpGet("~/Account/UpdateUsername")]
    public IActionResult UpdateUsernamePage()
    {
        return View("UpdateUsername");
    }

    [HttpGet("~/Account/UpdateAddress")]
    public IActionResult UpdateAddressPage()
    {
        return View("UpdateAddress");
    }

    [HttpPost("~/Account/Login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoginPagePost([FromForm] LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid)
        {
            return View("Login", model);
        }

        var dto = new LoginRequestDto { Email = model.Email, Password = model.Password, RememberMe = model.RememberMe };
        var result = await Login(dto) as OkObjectResult;
        if (result?.Value is not { } valueObj) return View("Login", model);

        var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(System.Text.Json.JsonSerializer.Serialize(valueObj));
        if (dict != null && dict.ContainsKey("success") && dict["success"] is bool success && !success)
        {
            ModelState.AddModelError(string.Empty, dict.ContainsKey("message") ? dict["message"].ToString()! : "Invalid login.");
            return View("Login", model);
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction("Index", "Home");
    }

    [HttpPost("~/Account/SignUp")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterPagePost([FromForm] SignUpViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("SignUp", model);
        }

        var dto = new RegisterRequestDto
        {
            Name = model.Name,
            Email = model.Email,
            DateOfBirth = model.DateOfBirth,
            Address = model.Address,
            Password = model.Password,
            ConfirmPassword = model.ConfirmPassword
        };

        var result = await Register(dto) as OkObjectResult;
        if (result?.Value is not { } valueObj) return View("SignUp", model);

        var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(System.Text.Json.JsonSerializer.Serialize(valueObj));
        if (dict != null && dict.ContainsKey("success") && dict["success"] is bool success && !success)
        {
            ModelState.AddModelError(string.Empty, dict.ContainsKey("message") ? dict["message"].ToString()! : "Registration failed.");
            return View("SignUp", model);
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpPost("~/Account/UpdateUsername")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateUsernamePagePost([FromForm] UpdateUsernameViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("UpdateUsername", model);
        }

        var result = await UpdateProfile(new UpdateProfileDto { Name = model.Username }) as OkObjectResult;
        if (result?.Value is not { } valueObj) return View("UpdateUsername", model);

        TempData["Success"] = "Username updated.";
        return RedirectToAction("ProfilePage");
    }

    [HttpPost("~/Account/UpdateAddress")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateAddressPagePost([FromForm] UpdateAddressViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("UpdateAddress", model);
        }

        var result = await UpdateProfile(new UpdateProfileDto { Address = model.Address }) as OkObjectResult;
        if (result?.Value is not { } valueObj) return View("UpdateAddress", model);

        TempData["Success"] = "Address updated.";
        return RedirectToAction("ProfilePage");
    }
}

public class LoginViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }
}

public class SignUpViewModel
{
    [Required]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Date of Birth")]
    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }

    [Required]
    [Display(Name = "Address")]
    public string Address { get; set; } = string.Empty;

    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class UpdateUsernameViewModel
{
    [Required]
    [Display(Name = "Username")]
    public string Username { get; set; } = string.Empty;
}

public class UpdateAddressViewModel
{
    [Required]
    [Display(Name = "Address")]
    public string Address { get; set; } = string.Empty;
}
