using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLCafeHV.Models
{
    public class OrderDetailModel
    {
        [Key]
        public int OrderDetailID { get; set; }

        [ForeignKey("Order")]
        public int OrderID { get; set; }
        public OrderModel Order { get; set; }

        [ForeignKey("Product")]
        public int ProductID { get; set; }
        public ProductModel Product { get; set; }

        public int Quantity { get; set; }

        public decimal TotalPrice { get; set; }
    }
}
