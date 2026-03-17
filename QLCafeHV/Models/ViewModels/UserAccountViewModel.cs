using System.ComponentModel.DataAnnotations;

namespace QLCafeHV.Models.ViewModels
{
    public class UserAccountViewModel
    {
        public int EmployeeID { get; set; }
        public string FullName { get; set; }
        public string? Phone { get; set; }
        public string Address { get; set; }
        public string? Cccd { get; set; }
        public string Username { get; set; }
        public EmployeeModel Employee { get; set; } = new EmployeeModel { Role = "User" };
        public AccountModel Account { get; set; } = new AccountModel { Role = "User" };

        public string Password { get; set; }
    
    }
}
