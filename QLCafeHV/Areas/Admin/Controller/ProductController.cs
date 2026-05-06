using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLCafeHV.Models;
using QLCafeHV.Models.DbConnect;
using QLCafeHV.Models.ViewModels;

namespace QLCafeHV.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly CoffeeContext _context;

        public ProductController(CoffeeContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
        }
        public IActionResult Index(string keyword,int page = 1)
        {
            int pageSize = 5;
            var query = _context.Products.Where(p => p.Status == 1).AsQueryable();
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(p =>
                  p.ProductID.ToString().Contains(keyword) ||
                  p.ProductName.Contains(keyword));
            }
            int totalItems = query.Count();
            var data = query
                .OrderByDescending(p => p.ProductID)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            var model = new PageViewModel<ProductModel>
            {
                Items = data,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            };
         
            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var product = _context.Products.FirstOrDefault(x => x.ProductID == id);

            if (product == null)
                return NotFound();

            return View(product);
        }
    }
}
