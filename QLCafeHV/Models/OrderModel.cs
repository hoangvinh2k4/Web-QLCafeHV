using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLCafeHV.Models
{
    public class OrderModel
    {
        [Key]
        public int OrderID { get; set; }

        [ForeignKey("Table")]
        public int? TableID { get; set; }
        public TableModel Table { get; set; }

        [ForeignKey("Employee")]
        public int EmployeeID { get; set; }
        public EmployeeModel Employee { get; set; }
      
        public string? OrderCode { get; set; }
        public DateTime OrderTime { get; set; } = DateTime.Now;
        public DateTime? CheckOutTime { get; set; }

        public decimal TotalAmount { get; set; }

        [StringLength(20)]
        public string Status { get; set; }
        public string? OrderType { get; set; }
        public string? KitchenStatus { get; set; }

        public ICollection<OrderDetailModel> OrderDetails { get; set; }
    }
}

