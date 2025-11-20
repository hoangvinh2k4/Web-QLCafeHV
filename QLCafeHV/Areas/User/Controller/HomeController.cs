using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace QLCafeHV.User.Controllers
{
    [Area("User")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "User")
            {
                return RedirectToAction("Login", "Auth");
            }
            return View();
        }
    }
}
