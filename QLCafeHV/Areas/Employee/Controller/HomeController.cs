using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLCafeHV.Helpers;
using QLCafeHV.Models;
using QLCafeHV.Models.DbConnect;
using QLCafeHV.Models.ViewModels;
using QLCafeHV.Services;
using static QLCafeHV.Models.ViewModels.PopupViewModel;


namespace QLCafeHV.Employee.Controllers
{
    [Area("Employee")]
    public class HomeController : Controller
    {
        private readonly ISalaryService _salaryService; // ← khai báo biến
        private readonly CoffeeContext _context;
        public HomeController(CoffeeContext context, ISalaryService salaryService)
        {
            _context = context;
            _salaryService = salaryService;
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
        public IActionResult CheckShiftTime(int shiftConfigId)
        {
            var shiftConfig = _context.AWorkShifts.FirstOrDefault(x => x.ShiftConfigId == shiftConfigId);

            if (shiftConfig == null)
                return Json(new { success = false });

            var now = DateTime.Now.TimeOfDay;

            if (now < shiftConfig.StartTime)
            {
                return Json(new
                {
                    success = true,
                    warningType = "early",
                    message = "Bạn đang mở ca sớm"
                });
            }

            if (now > shiftConfig.StartTime)
            {
                return Json(new
                {
                    success = true,
                    warningType = "late",
                    message = "Bạn đã mở ca muộn"
                });
            }

            return Json(new
            {
                success = true,
                warningType = "normal"
            });
        }
        [HttpPost]
        public IActionResult CheckCloseShiftTime()
        {
            int employeeId = HttpContext.Session.GetInt32("EmployeeID") ?? 0;

            if (employeeId == 0)
                return Json(new { success = false });

            var workShift = _context.EWorkShifts
                .FirstOrDefault(x => x.EmployeeID == employeeId && x.Status == "Opened");

            if (workShift == null)
                return Json(new { success = false, message = "Không có ca đang mở" });

            var shiftConfig = _context.AWorkShifts
                .FirstOrDefault(x => x.ShiftConfigId == workShift.ShiftConfigId);

            if (shiftConfig == null)
                return Json(new { success = false });

            var now = DateTime.Now.TimeOfDay;

            if (now < shiftConfig.EndTime)
            {
                return Json(new
                {
                    success = true,
                    warningType = "early",
                    message = "Bạn đang đóng ca sớm"
                });
            }

            if (now > shiftConfig.EndTime)
            {
                return Json(new
                {
                    success = true,
                    warningType = "late",
                    message = "Bạn đang đóng ca muộn"
                });
            }

            return Json(new
            {
                success = true,
                warningType = "normal"
            });
        }
        private void HandleLatePenalty(int employeeId, EWorkShiftModel workShift, AWorkShiftModel shiftConfig)
        {
            DateTime now = workShift.OpenTime;
            DateTime shiftStart = now.Date + shiftConfig.StartTime;

            if (now <= shiftStart)
                return; // Không đi muộn

            // Đếm số lần đi muộn trong tháng
            int lateCount = _context.SalaryAdjustments.Count(x =>
                x.EmployeeID == employeeId &&
                x.Reason == "Late" &&
                x.AdjustmentDate.Month == now.Month &&
                x.AdjustmentDate.Year == now.Year);

            decimal penaltyAmount = lateCount switch
            {
                0 => 50000,
                1 => 100000,
                _ => 200000
            };

            var adjustment = new SalaryAdjustmentModel
            {
                EmployeeID = employeeId,
                WorkShiftID = workShift.WorkShiftID,
                AdjustmentType = "Penalty",
                Reason = "Late",
                Amount = penaltyAmount,
                AdjustmentDate = now,
                Note = $"Đi muộn lần {lateCount + 1}"
            };

            _context.SalaryAdjustments.Add(adjustment);
        }
        private void HandleEarlyPenalty(int employeeId, EWorkShiftModel workShift, AWorkShiftModel shiftConfig)
        {
            if (!workShift.CloseTime.HasValue)
                return;

            DateTime actualEnd = workShift.CloseTime.Value;
            DateTime shiftEnd = workShift.OpenTime.Date + shiftConfig.EndTime;

            if (actualEnd >= shiftEnd)
                return; // Không đóng sớm

            var adjustment = new SalaryAdjustmentModel
            {
                EmployeeID = employeeId,
                WorkShiftID = workShift.WorkShiftID,
                AdjustmentType = "Penalty",
                Reason = "Early",
                Amount = 50000, // Bạn có thể tính tăng dần nếu muốn
                AdjustmentDate = DateTime.Now,
                Note = "Đóng ca sớm"
            };

            _context.SalaryAdjustments.Add(adjustment);
        }

        [HttpPost]
        public async Task<IActionResult> OpenShift(int shiftConfigId, decimal openAmount, string imageBase64)
        {
            int employeeId = HttpContext.Session.GetInt32("EmployeeID") ?? 0;
            if (employeeId == 0) return Json(new { success = false, message = "Chưa đăng nhập" });
            if (openAmount <= 0) return Json(new { success = false, message = "Phải nhập số tiền mở ca" });
            if (string.IsNullOrEmpty(imageBase64)) return Json(new { success = false, message = "Chưa chụp khuôn mặt" });

            try
            {
                bool hasOpenShift = _context.EWorkShifts.Any(x => x.EmployeeID == employeeId && x.Status == "Opened");
                if (hasOpenShift) return Json(new { success = false, message = "Bạn đang mở ca khác" });

                var shiftConfig = _context.AWorkShifts.FirstOrDefault(x => x.ShiftConfigId == shiftConfigId);
                if (shiftConfig == null) return Json(new { success = false, message = "Không tìm thấy ca" });

                // Lưu ảnh
                var parts = imageBase64.Split(',');
                if (parts.Length < 2) return Json(new { success = false, message = "Ảnh không hợp lệ" });

                byte[] imageBytes = Convert.FromBase64String(parts[1]);
                string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "faceshift");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                string fileName = $"face_{employeeId}_{DateTime.Now:yyyyMMddHHmmss}.png";
                string filePath = Path.Combine(folder, fileName);
                await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);
                string imagePath = "/faceshift/" + fileName;

                // Lưu workShift
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

                // Tính phạt đi muộn
                HandleLatePenalty(employeeId, workShift, shiftConfig);

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Mở ca thành công", imageUrl = imagePath });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CloseShift(decimal closeAmount, string imageBase64)
        {
            int employeeId = HttpContext.Session.GetInt32("EmployeeID") ?? 0;
            if (employeeId == 0) return Json(new { success = false, message = "Chưa đăng nhập" });
            if (closeAmount < 0) return Json(new { success = false, message = "Tiền đóng ca không hợp lệ" });
            if (string.IsNullOrEmpty(imageBase64)) return Json(new { success = false, message = "Chưa chụp khuôn mặt" });

            try
            {
                var workShift = _context.EWorkShifts.FirstOrDefault(x => x.EmployeeID == employeeId && x.Status == "Opened");
                if (workShift == null) return Json(new { success = false, message = "Không có ca đang mở" });

                var shiftConfig = _context.AWorkShifts.FirstOrDefault(x => x.ShiftConfigId == workShift.ShiftConfigId);
                if (shiftConfig == null) return Json(new { success = false, message = "Không tìm thấy ca" });

                // Lưu ảnh đóng ca
                var parts = imageBase64.Split(',');
                if (parts.Length < 2) return Json(new { success = false, message = "Ảnh không hợp lệ" });

                byte[] imageBytes = Convert.FromBase64String(parts[1]);
                string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "faceshift");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                string fileName = $"close_{employeeId}_{DateTime.Now:yyyyMMddHHmmss}.png";
                string filePath = Path.Combine(folder, fileName);
                await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);
                string imagePath = "/faceshift/" + fileName;

                // Cập nhật workShift
                workShift.CloseTime = DateTime.Now;
                workShift.CloseAmount = closeAmount;
                workShift.FaceImageClose = imagePath;
                workShift.Status = "Closed";

                // Tính giờ trong ca
                DateTime shiftStart = workShift.OpenTime.Date + shiftConfig.StartTime;
                DateTime shiftEnd = workShift.OpenTime.Date + shiftConfig.EndTime;
                DateTime actualStart = workShift.OpenTime;
                DateTime actualEnd = workShift.CloseTime.Value;

                DateTime start = actualStart > shiftStart ? actualStart : shiftStart;
                DateTime end = actualEnd < shiftEnd ? actualEnd : shiftEnd;
                double totalHours = (end - start).TotalHours;
                if (totalHours < 0) totalHours = 0;

                // Tính phạt đóng sớm
                HandleEarlyPenalty(employeeId, workShift, shiftConfig);             

                await _context.SaveChangesAsync();
                await _salaryService.CalculateSalaryAsync(employeeId, DateTime.Now.Month, DateTime.Now.Year);
                return Json(new
                {
                    success = true,
                    message = "Đóng ca thành công",
                    imageUrl = imagePath,
                    debugCloseTime = workShift.CloseTime.Value.TimeOfDay.ToString(),
                    debugEndTime = shiftConfig.EndTime.ToString()
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi server: {ex.Message}" });
            }
        }
    }
}
