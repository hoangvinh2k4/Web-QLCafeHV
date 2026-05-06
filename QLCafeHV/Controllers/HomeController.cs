using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLCafeHV.Models;
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
        public IActionResult LoadProducts(string category)
        {
            var products = _context.Products
                .Where(p => p.Status == 1)
                .ToList();

            ViewBag.Category = category;

            return PartialView("_ProductList", products);
        }
        public IActionResult ProductDetail(int id)
        {
            var product = _context.Products
                .FirstOrDefault(p => p.ProductID == id);

            if (product == null)
                return NotFound();

            // 👉 1. Sản phẩm cùng danh mục
            var relatedProducts = _context.Products
                .Where(p => p.Category == product.Category
                            && p.ProductID != product.ProductID
                            && p.Status == 1)
                .Take(4)
                .ToList();

            ViewBag.RelatedProducts = relatedProducts;


            // 👉 2. Sản phẩm bán chạy (TOP 5)
            var bestSeller = _context.OrderDetails
                .GroupBy(x => x.ProductID)
                .Select(g => new
                {
                    ProductID = g.Key,
                    TotalSold = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(5)
                .Join(_context.Products,
                      o => o.ProductID,
                      p => p.ProductID,
                      (o, p) => new ProductModel
                      {
                          ProductID = p.ProductID,
                          ProductName = p.ProductName,
                          Price = p.Price,
                          ImageUrl = p.ImageUrl,
                          Category = p.Category,
                          Status = p.Status
                      })
                .Where(p => p.Status == 1)
                .ToList();

            ViewBag.BestSeller = bestSeller;

            return View(product);
        }
    }
}
