using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Shop.Models;

namespace Shop.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager, ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = "/")
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = "/")
        {
            ViewData["ReturnUrl"] = returnUrl;
            _logger.LogInformation("Login attempt for email={Email}, RememberMe={RememberMe}", model.Email, model.RememberMe);
            if (ModelState.IsValid)
            {
                _logger.LogInformation("ModelState is valid, attempting PasswordSignInAsync");
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                _logger.LogInformation("PasswordSignInAsync result: Succeeded={Succeeded}, IsLockedOut={IsLockedOut}, RequiresTwoFactor={RequiresTwoFactor}", result.Succeeded, result.IsLockedOut, result.RequiresTwoFactor);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Login succeeded, redirecting to returnUrl={ReturnUrl}", returnUrl);
                    if (Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);
                    return RedirectToAction("Index", "Home");
                }
                _logger.LogWarning("Login failed for email={Email}", model.Email);
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
            else
            {
                _logger.LogWarning("Login ModelState invalid for email={Email}", model.Email);
                foreach (var kvp in ModelState)
                {
                    foreach (var error in kvp.Value.Errors)
                    {
                        _logger.LogWarning("ModelState error for {Key}: {Error}", kvp.Key, error.ErrorMessage);
                    }
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            _logger.LogInformation("Register attempt for email={Email}, UserRole={UserRole}", model.Email, model.UserRole);
            if (model.UserRole != "Vendor")
            {
                ModelState.Remove("CNIC");
                ModelState.Remove("ContactNumber");
                ModelState.Remove("ShopName");
                _logger.LogInformation("Removed vendor fields from ModelState for Customer registration");
            }
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    Address = model.Address,
                    CNIC = model.UserRole == "Vendor" ? model.CNIC : null,
                    ContactNumber = model.UserRole == "Vendor" ? model.ContactNumber : null,
                    ShopName = model.UserRole == "Vendor" ? model.ShopName : null
                };
                _logger.LogInformation("Creating user with UserName={UserName}, Email={Email}, FullName={FullName}", user.UserName, user.Email, user.FullName);
                var result = await _userManager.CreateAsync(user, model.Password);
                _logger.LogInformation("UserManager.CreateAsync result: Succeeded={Succeeded}, ErrorsCount={ErrorsCount}", result.Succeeded, result.Errors.Count());
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created successfully with Id={UserId}", user.Id);
                    if (model.UserRole == "Vendor")
                    {
                        _logger.LogInformation("Assigning Vendor role to user");
                        if (!await _roleManager.RoleExistsAsync("Vendor"))
                        {
                            await _roleManager.CreateAsync(new IdentityRole("Vendor"));
                        }
                        await _userManager.AddToRoleAsync(user, "Vendor");
                    }
                    _logger.LogInformation("Signing in user and redirecting to Home");
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }
                foreach (var error in result.Errors)
                {
                    _logger.LogWarning("User creation error: Code={Code}, Description={Description}", error.Code, error.Description);
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            else
            {
                _logger.LogWarning("Register ModelState invalid for email={Email}", model.Email);
                foreach (var kvp in ModelState)
                {
                    foreach (var error in kvp.Value.Errors)
                    {
                        _logger.LogWarning("ModelState error for {Key}: {Error}", kvp.Key, error.ErrorMessage);
                    }
                }
                _logger.LogWarning("Model values: UserRole={UserRole}, FullName={FullName}, Email={Email}, CNIC={CNIC}, ContactNumber={ContactNumber}, ShopName={ShopName}", model.UserRole, model.FullName, model.Email, model.CNIC, model.ContactNumber, model.ShopName);
                foreach (var key in ModelState.Keys) { _logger.LogInformation("ModelState key: {Key}, RawValue: {RawValue}", key, ModelState[key]?.RawValue?.ToString() ?? "(null)"); }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}