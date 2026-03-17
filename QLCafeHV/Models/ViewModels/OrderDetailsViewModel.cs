namespace QLCafeHV.Models.ViewModels
{
    public class OrderDetailsViewModel
    {
        public int OrderID { get; set; }
        public DateTime OrderTime { get; set; }
        public decimal TotalAmount { get; set; }

        // Nhân viên phục vụ
        public string EmployeeName { get; set; }
        // Tên bàn
        public string TableName { get; set; }

        // Chi tiết món
        public List<ProductOrderDetailsViewModel> Items { get; set; } = new();
    }
}
