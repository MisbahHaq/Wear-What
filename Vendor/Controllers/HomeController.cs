using Microsoft.AspNetCore.Mvc;

namespace Vendor.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
