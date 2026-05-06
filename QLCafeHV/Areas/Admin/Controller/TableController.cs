using Microsoft.AspNetCore.Mvc;
using QLCafeHV.Models;
using QLCafeHV.Models.DbConnect;
using QLCafeHV.Models.ViewModels;

namespace QLCafeHV.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TableController : Controller
    {
        private readonly CoffeeContext _context;

        public TableController(CoffeeContext context)
        {
            _context = context;
        }
        public IActionResult Index(string keyword ,int page = 1)
        {
            int pageSize = 5;
            var query = _context.Tables.AsQueryable();
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(p=>
                p.TableID.ToString().Contains(keyword) ||
                p.TableName.Contains(keyword));
            }
            int totalItems = query.Count();
            var data = query.OrderByDescending(p => p.TableID)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            var model = new PageViewModel<TableModel>
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
            var table = _context.Tables.FirstOrDefault(p => p.TableID == id);         
            return View(table);
        }
    }
}
