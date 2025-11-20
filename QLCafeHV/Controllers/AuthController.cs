using Microsoft.AspNetCore.Mvc;
using QLCafeHV.Models.DbConnect;
using QLCafeHV.Models;

namespace QLCafeHV.Controllers
{
    public class AuthController : Controller
    {
        private readonly CoffeeContext _context;

        public AuthController(CoffeeContext context)
        {
            _context = context;
        }

        // ========== ĐĂNG NHẬP ==========
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var acc = _context.Accounts
                .FirstOrDefault(a => a.Username == username && a.PasswordHash == password && a.IsActive);

            if (acc == null)
            {
                ViewBag.Error = "Sai tài khoản hoặc mật khẩu!";
                return View();
            }

            // Lưu session
            HttpContext.Session.SetString("Username", acc.Username);
            HttpContext.Session.SetString("Role", acc.Role);
            HttpContext.Session.SetString("User", acc.Role);

            // Chuyển hướng theo quyền
            if (acc.Role == "Admin")
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            else if (acc.Role == "Nhân viên")
                return RedirectToAction("Index", "Home", new { area = "Employee" });
            else
                return RedirectToAction("Index", "Home", new { area = "User" });
        }

        // ========== ĐĂNG KÝ ==========
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Register(string fullname, string phone, string username, string password, string role)
        {
            role = "User";  // 👉 Đặt mặc định là User

            if (_context.Accounts.Any(a => a.Username == username))
            {
                ViewBag.Error = "Tên đăng nhập đã tồn tại!";
                return View();
            }

            // Tạo Employee
            var emp = new EmployeeModel
            {
                FullName = fullname,
                Phone = phone,
                Role = role,
                Status = true
            };
            _context.Employees.Add(emp);
            _context.SaveChanges();

            // Tạo Account
            var acc = new AccountModel
            {
                EmployeeID = emp.EmployeeID,
                Username = username,
                PasswordHash = password,
                Role = role,
                IsActive = true
            };
            _context.Accounts.Add(acc);
            _context.SaveChanges();

            ViewBag.Success = "Đăng ký thành công! Mời bạn đăng nhập.";
            return RedirectToAction("Login");
        }
        public IActionResult Logout()
        {
            // Xóa session
            HttpContext.Session.Clear();

            // Chuyển hướng về action Login trong AuthController
            return RedirectToAction("Login", "Auth");
        }

    }
}
