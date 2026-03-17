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
                .Include(x => x.EWorkShifts.OrderByDescending(ws => ws.OpenTime)
        .Take(1))
                    .ThenInclude(ws => ws.Employee).OrderBy(x => x.StartTime)
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
        public IActionResult Edit(int id)
        {
            var shift = _context.AWorkShifts
                .FirstOrDefault(x => x.ShiftConfigId == id);

            if (shift == null)
                return NotFound();

            return View(shift);
        }
        [HttpPost]
        public IActionResult Edit(AWorkShiftModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var shift = _context.AWorkShifts
                .FirstOrDefault(x => x.ShiftConfigId == model.ShiftConfigId);

            if (shift == null)
                return NotFound();

            // Chỉ cập nhật giờ & ghi chú
            shift.StartTime = model.StartTime;
            shift.EndTime = model.EndTime;
            shift.Note = model.Note;

            _context.SaveChanges();

            TempData["Success"] = "Cập nhật giờ ca thành công";
            return RedirectToAction("Index");
        }

    }

}

