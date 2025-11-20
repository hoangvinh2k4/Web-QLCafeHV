using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLCafeHV.Models
{
    public class AccountModel
    {
        [Key]
        public int AccountID { get; set; }

        [ForeignKey("Employee")]
        public int EmployeeID { get; set; }
        public EmployeeModel Employee { get; set; }

        [Required, StringLength(50)]
        public string Username { get; set; }

        [Required, StringLength(255)]
        public string PasswordHash { get; set; }

        [Required]
        [RegularExpression("Admin|Nhân viên|User")]
        public string Role { get; set; }

        public bool IsActive { get; set; }
    }
}
