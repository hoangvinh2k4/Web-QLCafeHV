using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLCafeHV.Models
{
    [Table("ShiftConfigs")]
    public class AWorkShiftModel
    {
        [Key]
        public int ShiftConfigId { get; set; }
        [StringLength(20)]
        public string ShiftType { get; set; }

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        [StringLength(255)]
        public string? Note { get; set; }
        public ICollection<EWorkShiftModel>? EWorkShifts { get; set; }
    }
}
