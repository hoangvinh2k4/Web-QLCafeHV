using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLCafeHV.Models
{
    public class PaymentModel
    {
        [Key]
        public int PaymentID { get; set; }

        [ForeignKey("Orders")]
        public int OrderID { get; set; }
        public OrderModel? Orders { get; set; }
        public string PaymentMethod { get; set; }
        public decimal PaidAmount { get; set; }
        public DateTime PaymentTime { get; set; }
    }
}
