using Microsoft.AspNetCore.Mvc;
using QLCafeHV.Models.DbConnect;
using QLCafeHV.Models;
using Microsoft.AspNetCore.Identity;
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
            var acc = _context.Accounts.FirstOrDefault(a => a.Username == username);

            if (acc == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Sai tài khoản hoặc mật khẩu!"
                });
            }

            var passwordHasher = new PasswordHasher<object>();
            var result = passwordHasher.VerifyHashedPassword(null, acc.PasswordHash, password);

            if (result == PasswordVerificationResult.Failed)
            {
                return Json(new
                {
                    success = false,
                    message = "Sai tài khoản hoặc mật khẩu!"
                });
            }

            HttpContext.Session.SetString("Username", acc.Username ?? "");
            HttpContext.Session.SetString("Role", acc.Role ?? "");
            HttpContext.Session.SetInt32("EmployeeID", acc.EmployeeID);

            string redirectUrl;

            if (acc.Role == "Admin")
            {
                redirectUrl = Url.Action("Index", "Home", new { area = "Admin" })!;
            }
            else if (acc.Role == "Employee")
            {
                redirectUrl = Url.Action("Index", "Home", new { area = "Employee" })!;
            }
            else if (acc.Role == "User")
            {
                redirectUrl = Url.Action("Index", "Home")!;
            }
            else
            {
                return Json(new
                {
                    success = false,
                    message = "Role không hợp lệ!"
                });
            }

            return Json(new
            {
                success = true,
                redirectUrl
            });
        }

        // ========== ĐĂNG KÝ ==========
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Register(string fullname, string phone, string username,string cccd, string address, string email, string password)
        {
            string role = "User";

            if (_context.Accounts.Any(a => a.Username == username))
            {
                return Json(new { success = false, message = "Tên đăng nhập đã tồn tại!" });
            }

            if (_context.Employees.Any(e => e.Phone == phone))
            {
                return Json(new { success = false, message = "Số điện thoại đã tồn tại!" });
            }

            if (_context.Employees.Any(e => e.Cccd == cccd))
            {
                return Json(new { success = false, message = "Căn cước công dân đã tồn tại!" });
            }

            if (_context.Accounts.Any(e => e.Email == email))
            {
                return Json(new { success = false, message = "Email đã tồn tại!" });
            }

            var emp = new EmployeeModel
            {
                FullName = fullname,
                Phone = phone,
                Cccd = cccd,
                Address = address,
                Role = role,
                Status = 1
            };

            _context.Employees.Add(emp);
            _context.SaveChanges();

            var passwordHasher = new PasswordHasher<object>();

            var acc = new AccountModel
            {
                EmployeeID = emp.EmployeeID,
                Username = username,
                PasswordHash = passwordHasher.HashPassword(null, password),
                Email = email,
                Role = role,
                Status = 1
            };

            _context.Accounts.Add(acc);
            _context.SaveChanges();

            return Json(new { success = true, message = "Đăng ký thành công!" });
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Auth");
        }
        [HttpPost]
        public IActionResult CheckUsername(string username)
        {
            bool exists = _context.Accounts.Any(a => a.Username == username);

            return Json(new { exists });
        }
    }

}
