using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLCafeHV.Helpers;
using QLCafeHV.Models;
using QLCafeHV.Models.DbConnect;
using QLCafeHV.Models.ViewModels;
using static QLCafeHV.Models.ViewModels.PopupViewModel;


namespace QLCafeHV.Employee.Controllers
{
    [Area("Employee")]
    public class HomeController : Controller
    {
        private readonly CoffeeContext _context;
        public HomeController(CoffeeContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            int employeeId = HttpContext.Session.GetInt32("EmployeeID") ?? 0;
            if (employeeId == 0)
                return RedirectToAction("Login", "Account");

            bool hasOpenShift = _context.EWorkShifts
                .Any(x => x.EmployeeID == employeeId && x.Status == "Opened");

            var model = new PopupViewModel
            {
                HasOpenShift = hasOpenShift,

                // ✅ LẤY BÀN TỪ DB
                Tables = _context.Tables.ToList(),

                ShiftList = _context.AWorkShifts
                    .Select(s => new ShiftItem
                    {
                        ShiftConfigId = s.ShiftConfigId,
                        ShiftType = s.ShiftType
                    })
                    .ToList()
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult OpenShift(int shiftConfigId, decimal openAmount)
        {
            int employeeId = HttpContext.Session.GetInt32("EmployeeID") ?? 0;
            if (employeeId == 0)
                return Json(new { success = false, message = "Chưa đăng nhập" });

            // ❌ 1 nhân viên chỉ được mở 1 ca
            bool hasOpenShift = _context.EWorkShifts
                .Any(x => x.EmployeeID == employeeId && x.Status == "Opened");

            if (hasOpenShift)
                return Json(new { success = false, message = "Bạn đang mở ca khác" });

            var shiftConfig = _context.AWorkShifts
                .FirstOrDefault(x => x.ShiftConfigId == shiftConfigId);

            if (shiftConfig == null)
            {
                return Json(new { success = false, message = "Không tìm thấy ca" });
            }


            var workShift = new EWorkShiftModel
            {
                EmployeeID = employeeId,
                ShiftConfigId = shiftConfig.ShiftConfigId,
                ShiftType = shiftConfig.ShiftType, // lấy từ DB
                OpenAmount = openAmount,
                OpenTime = DateTime.Now,
                Status = "Opened"
            };

            _context.EWorkShifts.Add(workShift);
            _context.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult CloseShift(decimal closeAmount, string? note)
        {
            int employeeId = HttpContext.Session.GetInt32("EmployeeID") ?? 0;
            if (employeeId == 0)
                return Json(new { success = false, message = "Bạn chưa đăng nhập" });

            // 🔎 Lấy ca đang mở
            var shift = _context.EWorkShifts
                .FirstOrDefault(x => x.EmployeeID == employeeId && x.Status == "Opened");

            if (shift == null)
                return Json(new { success = false, message = "Không có ca đang mở" });

            // ❗ Validate
            if (closeAmount < 0)
                return Json(new { success = false, message = "Số tiền không hợp lệ" });

            // ✅ ĐÓNG CA
            shift.CloseAmount = closeAmount;
            shift.CloseTime = DateTime.Now;
            shift.Note = note;
            shift.Status = "Closed";

            _context.SaveChanges();

            // 🚪 LOGOUT
            HttpContext.Session.Clear();

            return Json(new { success = true });
        }
    }
}
