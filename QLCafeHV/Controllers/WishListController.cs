using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLCafeHV.Models;
using QLCafeHV.Models.DbConnect;
using QLCafeHV.Models.ViewModels;

namespace QLCafeHV.Controllers
{
    public class WishListController : Controller
    {
        private readonly CoffeeContext _context;
        public WishListController(CoffeeContext context)
        {
            _context = context;
        }
        public IActionResult WishList()
        {
            var model = new CartViewModel
            {
                Details = new List<CartDetailViewModel>()
            };

            return View(model);
        }
    }
}
