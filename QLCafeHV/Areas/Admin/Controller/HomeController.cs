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
           
            DateTime today = DateTime.Today;

                // Include Order và Product để load liên quan cùng lúc
                var orderDetailsToday = _context.OrderDetails
                    .Include(od => od.Order)
                    .Include(od => od.Product)
                    .Where(od => od.Order.OrderTime.Date == today)
                    .ToList();

                // Tổng sản phẩm đã bán hôm nay
                var totalItems = orderDetailsToday.Sum(od => od.Quantity);

                // Top 5 sản phẩm bán chạy hôm nay
                var topProducts = orderDetailsToday
                    .GroupBy(od => od.Product.ProductName)
                    .Select(g => new
                    {
                        ProductName = g.Key,
                        Quantity = g.Sum(x => x.Quantity)
                    })
                    .OrderByDescending(x => x.Quantity)
                    .Take(5)
                    .ToList();

                // Tổng đơn và doanh thu hôm nay
                var totalOrders = _context.Orders
                    .Count(o => o.OrderTime.Date == today);

                var totalRevenue = _context.Orders
                    .Where(o => o.OrderTime.Date == today)
                    .Sum(o => (decimal?)o.TotalAmount) ?? 0;

                // Doanh thu 7 ngày gần nhất
                var last7DaysRevenue = _context.Orders
                    .Where(o => o.OrderTime.Date >= DateTime.Today.AddDays(-6))
                    .GroupBy(o => o.OrderTime.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        Revenue = g.Sum(o => o.TotalAmount)
                    })
                    .OrderBy(g => g.Date)
                    .ToList();

                ViewBag.TotalOrders = totalOrders;
                ViewBag.TotalRevenue = totalRevenue;
                ViewBag.TotalItems = totalItems;
                ViewBag.TopProducts = topProducts;
                ViewBag.Last7DaysRevenue = last7DaysRevenue;

                return View();
            }
        }
    }


