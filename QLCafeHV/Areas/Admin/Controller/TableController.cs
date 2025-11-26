using Microsoft.AspNetCore.Mvc;
using QLCafeHV.Models;
using QLCafeHV.Models.DbConnect;

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
        public IActionResult Index()
        {
            var list = _context.Tables.ToList();
            return View(list);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(TableModel table)
        {
            try
            {
                table.Status = "Đang trống";
                table.CreatedTime = DateTime.Now;

                bool exists = _context.Tables.Any(t => t.TableName == table.TableName);
                if (exists)
                {
                    return Json(new { success = false, message = "Tên bàn đã tồn tại, vui lòng chọn tên khác!" });
                }
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Dữ liệu nhập chưa hợp lệ!" });

                _context.Tables.Add(table);
                _context.SaveChanges();

                return Json(new { success = true, message = "Thêm bàn thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var table = _context.Tables.FirstOrDefault(p => p.TableID == id);         
            return View(table);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, TableModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });

            var table = await _context.Tables.FindAsync(id);
            if (table == null)
                return Json(new { success = false, message = "Bàn không tồn tại!" });

            table.TableName = model.TableName;         
            table.Status = "Đang trống";         
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Cập nhật thành công!" });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var table = await _context.Tables.FindAsync(id);
            if (table == null)
                return Json(new { success = false, message = "Bàn không tồn tại!" });

            _context.Tables.Remove(table);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Xóa bàn thành công!" });
        }
    }
}
