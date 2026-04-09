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

        public ProductController(CoffeeContext context)
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

        [HttpPost]
        public IActionResult Create(ProductModel product)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Json(new { success = false, errors });
            }

            var file = Request.Form.Files.FirstOrDefault();
            if (file != null && file.Length > 0)
            {
                string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                string path = Path.Combine(folder, fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                product.ImageUrl = "/images/" + fileName;
            }
            else
            {
                return Json(new { success = false, message = "Vui lòng chọn ảnh sản phẩm!" });
            }

            product.CreatedTime = DateTime.Now;
            if (product.Status != 0 && product.Status != 1)
                product.Status = 1;

            try
            {
                _context.Products.Add(product);
                _context.SaveChanges();

                return Json(new { success = true, message = "Thêm sản phẩm thành công!" });
            }
            catch
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi khi thêm sản phẩm!" });
            }
        }


        [HttpGet]
        public IActionResult Edit(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.ProductID == id);
            if (product == null)
            {
                TempData["Error"] = "Sản phẩm không tồn tại!";
                return RedirectToAction("Index");
            }
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, ProductModel model)
        {
            if (id != model.ProductID)
                return Json(new { success = false, message = "Sai ID sản phẩm!" });

            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });

            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return Json(new { success = false, message = "Sản phẩm không tồn tại!" });

            // update field cơ bản
            product.ProductName = model.ProductName;
            product.Category = model.Category;
            product.Price = model.Price;
            product.Status = model.Status;
            product.UpdatedTime = DateTime.Now;

            // file upload
            var file = Request.Form.Files.FirstOrDefault();
            if (file != null)
            {
                string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                string filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                product.ImageUrl = "/images/" + fileName;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Cập nhật thành công!" });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return Json(new { success = false, message = "Sản phẩm không tồn tại!" });

            // Chỉ ngừng bán, không xóa thật
            product.Status = 0;
            product.UpdatedTime = DateTime.Now;

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã ngừng bán sản phẩm!" });
        }
    }
}
