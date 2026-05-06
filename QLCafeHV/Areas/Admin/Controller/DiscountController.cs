using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLCafeHV.Models;
using QLCafeHV.Models.DbConnect;
using QLCafeHV.Models.ViewModels;

namespace QLCafeHV.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DiscountController : Controller
    {
        private readonly CoffeeContext _context;

        public DiscountController(CoffeeContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var discounts = _context.Discounts.ToList();

            foreach (var item in discounts)
            {
                if (item.EndDate < DateTime.Today)
                {
                    item.Status = 0;
                }
            }

            _context.SaveChanges();

            return View(discounts);
        }

        public IActionResult Create()
        {
            return View();
        }

        public IActionResult Edit(int id)
        {
            var discount = _context.Discounts.Find(id);

            if (discount == null)
                return NotFound();

            return View(discount);
        }
    }
}