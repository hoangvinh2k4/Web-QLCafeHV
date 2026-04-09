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
        public IActionResult CalculateSalary(int month, int year)
        {
            var shifts = _context.EWorkShifts
                .Include(x => x.Employee)
                .Where(x => x.Status == "Closed"
                         && x.OpenTime.Month == month
                         && x.OpenTime.Year == year)
                .ToList();

            var grouped = shifts
                .GroupBy(x => x.EmployeeID)
                .Select(g =>
                {
                    var employee = g.First().Employee;

                    double totalHours = g.Sum(x =>
                        (x.CloseTime.Value - x.OpenTime).TotalHours);

                    decimal baseSalary =
                        (decimal)totalHours * employee.SalaryPerHour;

                    return new SalaryModel
                    {
                        EmployeeID = g.Key,
                        Month = month,
                        Year = year,
                        TotalHours = totalHours,
                        BaseSalary = baseSalary,
                        Bonus = 0,
                        Penalty = 0,
                        TotalSalary = baseSalary,
                        Status = "Unpaid"
                    };
                }).ToList();

            foreach (var item in grouped)
            {
                bool exists = _context.Salaries.Any(x =>
                    x.EmployeeID == item.EmployeeID &&
                    x.Month == month &&
                    x.Year == year);

                if (!exists)
                {
                    _context.Salaries.Add(item);
                }
            }

            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}