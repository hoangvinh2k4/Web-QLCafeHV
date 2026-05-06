using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLCafeHV.Models
{
    [Table("Reviews")]
    public class ReviewModel
    {
        [Key]
        public int ReviewId { get; set; }

        public int ProductId { get; set; }

        public string? CustomerName { get; set; }
        public string Email { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        public string? Comment { get; set; }

        public DateTime CreatedTime { get; set; } = DateTime.Now;

        public int Status { get; set; } = 1;
    }
}
