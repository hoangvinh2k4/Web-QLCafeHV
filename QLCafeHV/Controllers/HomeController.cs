using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLCafeHV.Models.DbConnect;

namespace QLCafeHV.Home.Controllers
{
    public class HomeController : Controller
    {
        private readonly CoffeeContext _context;

        public HomeController(CoffeeContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult LoadProducts()
        {
            var products = _context.Products.ToList();
            return PartialView("_ProductList", products);
        }
    }
}
