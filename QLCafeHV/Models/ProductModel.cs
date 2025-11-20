using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace QLCafeHV.Models
{
    public class ProductModel
    {
        [Key]
        public int ProductID { get; set; }

        [Required, StringLength(100)]
        public string ProductName { get; set; }

        [StringLength(50)]
        public string Category { get; set; }

        [StringLength(255)]
        public string? ImageUrl { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }


        [Required]
        public decimal Price { get; set; }

        [Required]
        [RegularExpression("Còn bán|Ngừng bán")]
        public string Status { get; set; }    
    }
}
