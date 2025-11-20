using System.ComponentModel.DataAnnotations;

namespace QLCafeHV.Models
{
    public class TableModel
    {
        [Key]
        public int TableID { get; set; }

        [Required, StringLength(50)]
        public string TableName { get; set; }

        [Required]
        [RegularExpression("Đang trống|Đang phục vụ")]
        public string Status { get; set; }
    }
}
