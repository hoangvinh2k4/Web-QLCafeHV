using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLCafeHV.Models
{
    public class OrderDeliveryModel
    {
        [Key]
        public int DeliveryID { get; set; }
        public int OrderID { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string District { get; set; }
        public decimal ShippingFee { get; set; }
        public string? Note { get; set; }
    }
}

