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

        [HttpPost]
        public IActionResult Create(DiscountModel model)
        {
            model.Code = "SALE" + DateTime.Now.Ticks.ToString().Substring(10);

            model.Status = 1;
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                foreach (var error in errors)
                {
                    Console.WriteLine(error);
                }

                return View(model);
            }        

            _context.Discounts.Add(model);
            _context.SaveChanges();

            TempData["success"] = "Tạo mã giảm giá thành công";

            return RedirectToAction("Index");
        }
        public IActionResult Edit(int id)
        {
            var discount = _context.Discounts.Find(id);

            if (discount == null)
                return NotFound();

            return View(discount);
        }
        [HttpPost]
        public IActionResult Edit(DiscountModel model)
        {
            ModelState.Remove("Code");

            if (!ModelState.IsValid)
                return View(model);

            var discount = _context.Discounts.Find(model.DiscountId);

            if (discount == null)
                return NotFound();

            discount.PercentValue = model.PercentValue;
            discount.Quantity = model.Quantity;
            discount.StartDate = model.StartDate;
            discount.EndDate = model.EndDate;
            discount.Status = model.Status;

            _context.SaveChanges();

            TempData["success"] = "Cập nhật mã giảm giá thành công";

            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var discount = _context.Discounts.Find(id);

            if (discount == null)
                return NotFound();

            discount.Status = 0;

            _context.SaveChanges();

            TempData["success"] = "Khóa mã giảm giá thành công";

            return RedirectToAction("Index");
        }
    }
}