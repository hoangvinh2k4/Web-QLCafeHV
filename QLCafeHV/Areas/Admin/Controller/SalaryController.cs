using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLCafeHV.Models;
using QLCafeHV.Models.DbConnect;
using QLCafeHV.Models.ViewModels;

namespace QLCafeHV.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SalaryController : Controller
    {
        private readonly CoffeeContext _context;

        public SalaryController(CoffeeContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var salaries = _context.Salaries
                .Include(x => x.Employee)
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Month)
                .ToList();

            return View(salaries);
        }
        public IActionResult GetSalaryDetail(int employeeId, int month, int year)
        {
            var details = _context.SalaryAdjustments
                .Where(x => x.EmployeeID == employeeId
                         && x.AdjustmentDate.Month == month
                         && x.AdjustmentDate.Year == year)
                .ToList();

            return PartialView("_SalaryDetailPartial", details);
        }

        [HttpPost]
        public IActionResult CalculateSalary(int month, int year, int? employeeId = null)
        {
            // Lấy ca đã đóng
            var shiftsQuery = _context.EWorkShifts
                .Include(x => x.Employee)
                .Where(x => x.Status == "Closed"
                         && x.OpenTime.Month == month
                         && x.OpenTime.Year == year);

            if (employeeId.HasValue)
                shiftsQuery = shiftsQuery.Where(x => x.EmployeeID == employeeId.Value);

            var shifts = shiftsQuery.ToList();

            var grouped = shifts
                .GroupBy(x => x.EmployeeID)
                .Select(g =>
                {
                    var employee = g.First().Employee;

                    double totalHours = g.Sum(x =>
                    {
                        var shiftConfig = _context.AWorkShifts.FirstOrDefault(s => s.ShiftConfigId == x.ShiftConfigId);
                        if (shiftConfig == null || x.CloseTime == null) return 0;

                        DateTime shiftStart = x.OpenTime.Date + shiftConfig.StartTime;
                        DateTime shiftEnd = x.OpenTime.Date + shiftConfig.EndTime;

                        DateTime actualStart = x.OpenTime;
                        DateTime actualEnd = x.CloseTime.Value;

                        DateTime start = actualStart > shiftStart ? actualStart : shiftStart;
                        DateTime end = actualEnd < shiftEnd ? actualEnd : shiftEnd;

                        return Math.Max((end - start).TotalHours, 0);
                    });

                    decimal baseSalary = (decimal)totalHours * employee.SalaryPerHour;

                    decimal bonus = _context.SalaryAdjustments
                        .Where(a => a.EmployeeID == g.Key
                                 && a.AdjustmentType == "Bonus"
                                 && a.AdjustmentDate.Month == month
                                 && a.AdjustmentDate.Year == year)
                        .Sum(a => (decimal?)a.Amount) ?? 0;

                    decimal penalty = _context.SalaryAdjustments
                        .Where(a => a.EmployeeID == g.Key
                                 && a.AdjustmentType == "Penalty"
                                 && a.AdjustmentDate.Month == month
                                 && a.AdjustmentDate.Year == year)
                        .Sum(a => (decimal?)a.Amount) ?? 0;

                    decimal totalSalary = baseSalary + bonus - penalty;

                    // Cập nhật hoặc thêm mới salary
                    var salary = _context.Salaries.FirstOrDefault(x =>
                        x.EmployeeID == g.Key &&
                        x.Month == month &&
                        x.Year == year);

                    if (salary == null)
                    {
                        salary = new SalaryModel
                        {
                            EmployeeID = g.Key,
                            Month = month,
                            Year = year,
                            TotalHours = totalHours,
                            BaseSalary = baseSalary,
                            Bonus = bonus,
                            Penalty = penalty,
                            TotalSalary = totalSalary,
                            Status = "Unpaid"
                        };
                        _context.Salaries.Add(salary);
                    }
                    else
                    {
                        salary.TotalHours = totalHours;
                        salary.BaseSalary = baseSalary;
                        salary.Bonus = bonus;
                        salary.Penalty = penalty;
                        salary.TotalSalary = totalSalary;
                    }

                    return salary;
                }).ToList();

            _context.SaveChanges();

            return Json(new { success = true });
        }
    }
}