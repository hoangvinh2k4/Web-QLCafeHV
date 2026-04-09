using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLCafeHV.Models
{

    public class SalaryModel
    {
        [Key]
        public int SalaryID { get; set; }

        [Required]
        public int EmployeeID { get; set; }

        [Required]
        public int Month { get; set; }

        [Required]
        public int Year { get; set; }

        [Required]
        public double TotalHours { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal BaseSalary { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Bonus { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Penalty { get; set; } = 0;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalSalary { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Unpaid";

        [ForeignKey("EmployeeID")]
        public EmployeeModel Employee { get; set; }
    }
}

