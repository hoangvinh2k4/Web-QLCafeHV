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

        [StringLength(20)]
        public string Phone { get; set; }

        [Required]
        [RegularExpression("Admin|Nhân viên|User")]
        public string Role { get; set; }

        public bool Status { get; set; }

        // Quan hệ với tài khoản
        public AccountModel Account { get; set; }

        // Quan hệ với Orders
        public ICollection<OrderModel> Orders { get; set; }
    }
}

