using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLCafeHV.Models
{
    [Table("DiscountCodes")]
    public class DiscountModel
    {
        [Key]
        public int DiscountId { get; set; }

        public string? Code { get; set; }

        [Required(ErrorMessage = "Nhập phần trăm giảm")]
        [Range(1, 100, ErrorMessage = "Phần trăm từ 1 đến 100")]
        public int PercentValue { get; set; }

        [Required(ErrorMessage = "Nhập số lượng")]
        public int Quantity { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public int Status { get; set; }
    }
}

