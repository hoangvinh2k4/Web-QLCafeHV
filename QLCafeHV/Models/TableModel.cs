using System.ComponentModel.DataAnnotations;

namespace QLCafeHV.Models
{
    public class TableModel
    {
        [Key]
        public int TableID { get; set; }

        [Required, StringLength(50)]
        public string TableName { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
    }
}
