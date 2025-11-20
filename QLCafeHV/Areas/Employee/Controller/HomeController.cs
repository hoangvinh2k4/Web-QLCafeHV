using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace QLCafeHV.Employee.Controllers
{
    [Area("Employee")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Nhân viên")
            {
                return RedirectToAction("Login", "Auth");
            }
            return View();
        }
    }
}
