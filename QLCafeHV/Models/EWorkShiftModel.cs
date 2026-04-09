using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLCafeHV.Models
{
    [Table("WorkShifts")]
    public class EWorkShiftModel
    {
        [Key]
        public int WorkShiftID { get; set; }
        public int EmployeeID { get; set; }
        public decimal OpenAmount { get; set; }
        public string ShiftType { get; set; } // Ca sáng / Ca chiều / Ca tối
        public int ShiftConfigId { get; set; }
        public DateTime OpenTime { get; set; }
        public decimal? CloseAmount { get; set; }
        public DateTime? CloseTime { get; set; }
        public string? FaceImageOpen { get; set; }
        public string? FaceImageClose { get; set; }
        public string? Note { get; set; }
        public string Status { get; set; } // Open / Closed
        
        [ForeignKey(nameof(EmployeeID))]
        public EmployeeModel Employee { get; set; }
        [ForeignKey(nameof(ShiftConfigId))]
        public AWorkShiftModel AWorkShift { get; set; }
    }
}
