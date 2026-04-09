using QLCafeHV.Models;

namespace QLCafeHV.Services
{
    public interface ISalaryService
    {
        Task<SalaryModel> CalculateSalaryAsync(int employeeId, int month, int year);
    }
}