using System.ComponentModel.DataAnnotations;

namespace QLCafeHV.Models.ViewModels
{
    public class EmployeeAccountViewModel
    {
        public int EmployeeID { get; set; }
        public string FullName { get; set; }
        public string? Phone { get; set; }
        public string Address { get; set; }
        public string? Cccd { get; set; }
        public string Username { get; set; }
        public EmployeeModel Employee { get; set; } = new EmployeeModel { Role = "Employee" };
        public AccountModel Account { get; set; } = new AccountModel { Role = "Employee" };

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp!")]
        public string ConfirmPassword { get; set; }
    }
}
