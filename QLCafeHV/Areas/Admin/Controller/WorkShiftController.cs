using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLCafeHV.Models;
using QLCafeHV.Models.DbConnect;
using QLCafeHV.Models.ViewModels;

namespace QLCafeHV.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class WorkShiftController : Controller
    {
        private readonly CoffeeContext _context;

        public WorkShiftController(CoffeeContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var configs = _context.AWorkShifts
                .Include(x => x.EWorkShifts
                    .Where(ws => ws.Status == "Opened")
                    .OrderByDescending(ws => ws.OpenTime))
                .ThenInclude(ws => ws.Employee)
                .OrderBy(x => x.StartTime)
                .ToList();

            var model = configs.Select(c => new ShiftAdminViewModel
            {
                ShiftId = c.ShiftConfigId,
                ShiftType = c.ShiftType,
                StartTime = c.StartTime,
                EndTime = c.EndTime,
                Note = c.Note,
                EWorkShifts = c.EWorkShifts
            }).ToList();

            return View(model);
        }
        public IActionResult GetShiftImageByDate(int employeeId, DateTime date)
        {
            var shift = _context.EWorkShifts
                .Where(x => x.EmployeeID == employeeId && x.OpenTime.Date == date.Date)
                .FirstOrDefault();

            return Json(new
            {
                openImage = shift?.FaceImageOpen,
                closeImage = shift?.FaceImageClose
            });
        }
    }

}

