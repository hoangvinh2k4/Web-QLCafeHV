using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLCafeHV.Models.DbConnect;

namespace QLCafeHV.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        private readonly CoffeeContext _context;

        public HomeController(CoffeeContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin")
            {
                return RedirectToAction("Login", "Auth");
            }

            var today = DateTime.Today;

            // 1. Load payments hôm nay
            var paymentsToday = _context.Payments
                .Include(p => p.Orders)
                    .ThenInclude(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                .Where(p => p.PaymentTime.Date == today)
                .ToList();

            // 2. Tổng sản phẩm bán hôm nay
            var totalItems = paymentsToday
                .SelectMany(p => p.Orders.OrderDetails)
                .Sum(od => od.Quantity);

            // 3. Top 5 sản phẩm bán chạy hôm nay
            var topProducts = paymentsToday
                .SelectMany(p => p.Orders.OrderDetails)
                .GroupBy(od => od.Product.ProductName)
                .Select(g => new
                {
                    ProductName = g.Key,
                    Quantity = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.Quantity)
                .Take(5)
                .ToList();

            // 4. Tổng đơn đã thanh toán hôm nay
            var totalOrders = paymentsToday
                .Select(p => p.OrderID)
                .Distinct()
                .Count();

            // 5. Tổng doanh thu hôm nay
            var totalRevenue = paymentsToday.Sum(p => p.PaidAmount);

            // 6. Doanh thu từng ngày trong tháng hiện tại
            var daysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);

            var dailyRevenue = Enumerable.Range(1, daysInMonth)
                .Select(day => new
                {
                    Date = day,
                    Revenue = _context.Payments
                        .Where(p => p.PaymentTime.Day == day
                                 && p.PaymentTime.Month == DateTime.Now.Month
                                 && p.PaymentTime.Year == DateTime.Now.Year)
                        .Sum(p => (decimal?)p.PaidAmount) ?? 0
                })
                .ToList();

            // 7. Doanh thu 12 tháng trong năm
            var monthlyRevenue = Enumerable.Range(1, 12)
                .Select(month => new
                {
                    Month = month,
                    Revenue = _context.Payments
                        .Where(p => p.PaymentTime.Month == month
                                 && p.PaymentTime.Year == DateTime.Now.Year)
                        .Sum(p => (decimal?)p.PaidAmount) ?? 0
                })
                .ToList();

            // 8. Truyền dữ liệu sang View
            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalItems = totalItems;
            ViewBag.TopProducts = topProducts;
            ViewBag.Last7DaysRevenue = dailyRevenue;
            ViewBag.MonthlyRevenue = monthlyRevenue;

            return View();
        }

        [HttpGet]
        public IActionResult GetRevenueByMonth(int month)
        {
            var daysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, month);

            var data = Enumerable.Range(1, daysInMonth)
                .Select(day => new
                {
                    Day = day,
                    Revenue = _context.Payments
                        .Where(p => p.PaymentTime.Year == DateTime.Now.Year
                                 && p.PaymentTime.Month == month
                                 && p.PaymentTime.Day == day)
                        .Sum(p => (decimal?)p.PaidAmount) ?? 0
                })
                .ToList();

            return Json(data);
        }
    }
}