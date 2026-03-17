using Microsoft.AspNetCore.Mvc;
using QLCafeHV.Models;
using QLCafeHV.Models.DbConnect;
using QLCafeHV.Models.ViewModels;

namespace QLCafeHV.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class EmployeeController : Controller
    {
        private readonly CoffeeContext _context;
        public EmployeeController(CoffeeContext context)
        {
            _context = context;
        }
        public IActionResult Index(string keyword, int page = 1)
        {
            int pageSize = 5;

            // Lấy query nhân viên
            var data = _context.Employees
            .Where(e => e.Role == "Employee" && e.Status == 1);

            // Áp dụng tìm kiếm nếu có keyword
            if (!string.IsNullOrEmpty(keyword))
            {
                data = data.Where(e =>
                    e.EmployeeID.ToString().Contains(keyword) ||
                    e.FullName.Contains(keyword));
            }

            // Tính tổng số item sau khi filter
            int totalItems = data.Count();

            // Lấy dữ liệu phân trang và map sang ViewModel
            var items = data
                .OrderBy(e => e.EmployeeID) 
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new EmployeeAccountViewModel
                {
                    Employee = new EmployeeModel
                    {
                        EmployeeID = e.EmployeeID,
                        FullName = e.FullName,
                        Phone = e.Phone,
                        Address = e.Address,
                        Cccd = e.Cccd,
                        Role = e.Role
                    },
                    Account = new AccountModel
                    {
                        Username = e.Account.Username,
                        PasswordHash = e.Account.PasswordHash,
                        Role = e.Account.Role
                    }
                })
                .ToList();

            // Chuẩn bị model cho view
            var model = new PageViewModel<EmployeeAccountViewModel>
            {
                Items = items,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            };

            ViewBag.Keyword = keyword;

            return View(model);
        }
        public IActionResult Create()
        {
            var viewmodel = new EmployeeAccountViewModel
            {
                Employee = new EmployeeModel(),
                Account = new AccountModel()
            };

            return View(viewmodel);
        }
        [HttpPost]
        public IActionResult Create(EmployeeAccountViewModel model)
        {
            if (string.IsNullOrEmpty(model.Employee.FullName))
                return Json(new { success = false, message = "Vui lòng nhập tên nhân viên" });

            if (string.IsNullOrEmpty(model.Account.Username))
                return Json(new { success = false, message = "Vui lòng nhập tên đăng nhập" });
            
            if (string.IsNullOrEmpty(model.Employee.Cccd))
                return Json(new { success = false, message = "Vui lòng nhập số căn cước công dân" });
            
            if (string.IsNullOrEmpty(model.Employee.Address))
                return Json(new { success = false, message = "Vui lòng nhập địa chỉ" });

            if (string.IsNullOrEmpty(model.Password))
                return Json(new { success = false, message = "Vui lòng nhập mật khẩu" });

            if (model.Password != model.ConfirmPassword)
                return Json(new { success = false, message = "Xác nhận mật khẩu không đúng" });

           
            if (_context.Accounts.Any(a => a.Username == model.Account.Username))
                return Json(new { success = false, message = "Tên đăng nhập đã tồn tại!" });

            
            _context.Employees.Add(model.Employee);
            _context.SaveChanges();

      
            model.Account.EmployeeID = model.Employee.EmployeeID;
            model.Account.PasswordHash = model.Password;
            _context.Accounts.Add(model.Account);
            _context.SaveChanges();

            return Json(new { success = true, message = "Thêm nhân viên thành công!" });
        }

        public IActionResult Edit(int id)
        {
            var employee = _context.Employees.FirstOrDefault(e => e.EmployeeID == id);
            if (employee == null)
                return NotFound();

            return View(employee);
        }

        [HttpPost]
        public IActionResult Edit(EmployeeModel model)
        {
            if (string.IsNullOrEmpty(model.FullName))
                return Json(new { success = false, message = "Vui lòng nhập tên nhân viên" });

            if (string.IsNullOrEmpty(model.Cccd))
                return Json(new { success = false, message = "Vui lòng nhập số căn cước công dân" });

            if (string.IsNullOrEmpty(model.Address))
                return Json(new { success = false, message = "Vui lòng nhập địa chỉ" });

            var employee = _context.Employees.FirstOrDefault(e => e.EmployeeID == model.EmployeeID);
            if (employee == null)
                return Json(new { success = false, message = "Không tìm thấy nhân viên!" });

            // Cập nhật
            employee.FullName = model.FullName;
            employee.Phone = model.Phone;
            employee.Address = model.Address;
            employee.Cccd = model.Cccd;

            _context.SaveChanges();

            return Json(new { success = true, message = "Cập nhật thông tin thành công!" });
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var employee = _context.Employees.Find(id);

            if (employee == null)
            {
                return Json(new { success = false, message = "Nhân viên không tồn tại!" });
            }

            employee.Status = 0;

            var account = _context.Accounts.FirstOrDefault(a => a.EmployeeID == id);

            if (account != null)
            {
                account.Status = 0;
            }

            _context.SaveChanges();

            return Json(new { success = true, message = "Nhân viên đã nghỉ việc!" });
        }

    }
}
