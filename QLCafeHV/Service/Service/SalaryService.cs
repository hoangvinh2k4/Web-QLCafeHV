using Microsoft.EntityFrameworkCore;
using QLCafeHV.Models;
using QLCafeHV.Models.DbConnect;
using QLCafeHV.Models.ViewModels;

namespace QLCafeHV.Services
{
    public class SalaryService : ISalaryService
    {
        private readonly CoffeeContext _context;

        public SalaryService(CoffeeContext context)
        {
            _context = context;
        }

        public async Task<SalaryModel> CalculateSalaryAsync(int employeeId, int month, int year)
        {
            var shifts = await _context.EWorkShifts
                .Include(x => x.Employee)
                .Where(x => x.Status == "Closed" &&
                            x.EmployeeID == employeeId &&
                            x.OpenTime.Month == month &&
                            x.OpenTime.Year == year)
                .ToListAsync();

            if (!shifts.Any()) return null;

            var employee = shifts.First().Employee;

            double totalHours = shifts.Sum(x =>
            {
                var shiftConfig = _context.AWorkShifts
                    .FirstOrDefault(s => s.ShiftConfigId == x.ShiftConfigId);

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
                .Where(a => a.EmployeeID == employeeId &&
                            a.AdjustmentType == "Bonus" &&
                            a.AdjustmentDate.Month == month &&
                            a.AdjustmentDate.Year == year)
                .Sum(a => (decimal?)a.Amount) ?? 0;

            decimal penalty = _context.SalaryAdjustments
                .Where(a => a.EmployeeID == employeeId &&
                            a.AdjustmentType == "Penalty" &&
                            a.AdjustmentDate.Month == month &&
                            a.AdjustmentDate.Year == year)
                .Sum(a => (decimal?)a.Amount) ?? 0;

            decimal totalSalary = baseSalary + bonus - penalty;

            var salary = await _context.Salaries
                .FirstOrDefaultAsync(s => s.EmployeeID == employeeId &&
                                          s.Month == month &&
                                          s.Year == year);

            if (salary == null)
            {
                salary = new SalaryModel
                {
                    EmployeeID = employeeId,
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

            await _context.SaveChangesAsync();
            return salary;
        }
    }
}