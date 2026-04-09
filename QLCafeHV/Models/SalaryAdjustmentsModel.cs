using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLCafeHV.Models
{
    public class SalaryAdjustmentModel
    {
        [Key]
        public int AdjustmentID { get; set; }

        [Required]
        public int EmployeeID { get; set; }

        public int? WorkShiftID { get; set; }

        [StringLength(20)]
        public string AdjustmentType { get; set; }   // Bonus / Penalty

        [StringLength(100)]
        public string Reason { get; set; }           // Late / Early / OT

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime AdjustmentDate { get; set; } = DateTime.Now;

        [StringLength(255)]
        public string? Note { get; set; }

        // Navigation
        [ForeignKey("EmployeeID")]
        public virtual EmployeeModel Employee { get; set; }

        [ForeignKey("WorkShiftID")]
        public virtual EWorkShiftModel? WorkShift { get; set; }
    }
}

