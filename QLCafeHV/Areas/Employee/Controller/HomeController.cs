using System.Text.Json;
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
        public async Task<IActionResult> OpenShift(int shiftConfigId, decimal openAmount, string imageBase64)
        {
            int employeeId = HttpContext.Session.GetInt32("EmployeeID") ?? 0;

            if (employeeId == 0)
                return Json(new { success = false, message = "Chưa đăng nhập" });

            if (openAmount <= 0)
                return Json(new { success = false, message = "Phải nhập số tiền mở ca" });

            if (string.IsNullOrEmpty(imageBase64))
                return Json(new { success = false, message = "Chưa chụp khuôn mặt" });

            // ================= CHECK OPEN SHIFT =================
            bool hasOpenShift = _context.EWorkShifts.Any(x => x.EmployeeID == employeeId && x.Status == "Opened");

            if (hasOpenShift)
                return Json(new { success = false, message = "Bạn đang mở ca khác" });

            var shiftConfig = _context.AWorkShifts.FirstOrDefault(x => x.ShiftConfigId == shiftConfigId);

            if (shiftConfig == null)
                return Json(new { success = false, message = "Không tìm thấy ca" });

            // ================= SAVE IMAGE =================
            string imagePath = "";

            try
            {
                var parts = imageBase64.Split(',');

                if (parts.Length < 2)
                    return Json(new { success = false, message = "Ảnh không hợp lệ" });

                var base64Data = parts[1];
                byte[] imageBytes = Convert.FromBase64String(base64Data);

                string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "faceshift");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string fileName = $"face_{employeeId}_{DateTime.Now:yyyyMMddHHmmss}.png";

                string filePath = Path.Combine(folder, fileName);

                await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);

                imagePath = "/faceshift/" + fileName;
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi lưu ảnh: {ex.Message}" });
            }

            // ================= SAVE DB =================
            var workShift = new EWorkShiftModel
            {
                EmployeeID = employeeId,
                ShiftConfigId = shiftConfig.ShiftConfigId,
                ShiftType = shiftConfig.ShiftType,
                OpenAmount = openAmount,
                OpenTime = DateTime.Now,
                Status = "Opened",
                FaceImageOpen = imagePath
            };

            _context.EWorkShifts.Add(workShift);

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Mở ca thành công",
                imageUrl = imagePath
            });
        }
        [HttpPost]
        public async Task<IActionResult> CloseShift(decimal closeAmount, string imageBase64)
        {
            int employeeId = HttpContext.Session.GetInt32("EmployeeID") ?? 0;

            if (employeeId == 0)
                return Json(new { success = false, message = "Chưa đăng nhập" });

            if (closeAmount < 0)
                return Json(new { success = false, message = "Tiền đóng ca không hợp lệ" });

            if (string.IsNullOrEmpty(imageBase64))
                return Json(new { success = false, message = "Chưa chụp khuôn mặt" });

            // ================= CHECK OPEN SHIFT =================
            var workShift = _context.EWorkShifts
                .FirstOrDefault(x => x.EmployeeID == employeeId && x.Status == "Opened");

            if (workShift == null)
                return Json(new { success = false, message = "Không có ca đang mở" });

            // ================= SAVE IMAGE =================
            string imagePath = "";

            try
            {
                var parts = imageBase64.Split(',');

                if (parts.Length < 2)
                    return Json(new { success = false, message = "Ảnh không hợp lệ" });

                var base64Data = parts[1];
                byte[] imageBytes = Convert.FromBase64String(base64Data);

                string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "faceshift");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string fileName = $"close_{employeeId}_{DateTime.Now:yyyyMMddHHmmss}.png";

                string filePath = Path.Combine(folder, fileName);

                await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);

                imagePath = "/faceshift/" + fileName;
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi lưu ảnh: {ex.Message}" });
            }

            // ================= UPDATE SHIFT =================
            workShift.CloseTime = DateTime.Now;
            workShift.CloseAmount = closeAmount;
            workShift.FaceImageClose = imagePath;
            workShift.Status = "Closed";

            //// ================= TÍNH GIỜ =================
            double totalHours = (workShift.CloseTime.Value - workShift.OpenTime).TotalHours;

            // ================= LẤY NHÂN VIÊN =================
            var employee = _context.Employees.FirstOrDefault(x => x.EmployeeID == employeeId);

            if (employee != null)
            {
                decimal baseSalary = (decimal)totalHours * employee.SalaryPerHour;

                var salary = _context.Salaries.FirstOrDefault(x =>
                    x.EmployeeID == employeeId &&
                    x.Month == DateTime.Now.Month &&
                    x.Year == DateTime.Now.Year);

                if (salary == null)
                {
                    salary = new SalaryModel
                    {
                        EmployeeID = employeeId,
                        Month = DateTime.Now.Month,
                        Year = DateTime.Now.Year,
                        TotalHours = totalHours,
                        BaseSalary = baseSalary,
                        Bonus = 0,
                        Penalty = 0,
                        TotalSalary = baseSalary,
                        Status = "Unpaid"
                    };

                    _context.Salaries.Add(salary);
                }
                else
                {
                    salary.TotalHours += totalHours;
                    salary.BaseSalary += baseSalary;
                    salary.TotalSalary = salary.BaseSalary + salary.Bonus - salary.Penalty;
                }
            }

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Đóng ca thành công",
                imageUrl = imagePath
            });
        }
    }
}
