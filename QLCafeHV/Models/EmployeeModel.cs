using System.ComponentModel.DataAnnotations;
using System.Security.Principal;

namespace QLCafeHV.Models
{
    public class EmployeeModel
    {
        [Key]
        public int EmployeeID { get; set; }

        [Required, StringLength(100)]
        public string FullName { get; set; }

        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Cccd { get; set; }
        [Required]
        [RegularExpression("Admin|Employee|User")]
        public string Role { get; set; }
        [Required]
        [Range(0, 1, ErrorMessage = "Status chỉ nhận 0 hoặc 1")]
        public int Status { get; set; }
        // Quan hệ với tài khoản
        public AccountModel Account { get; set; }

        // Quan hệ với Orders
        public ICollection<OrderModel> Orders { get; set; }
        public ICollection<EWorkShiftModel> EWorkShifts { get; set; }
    }
}

