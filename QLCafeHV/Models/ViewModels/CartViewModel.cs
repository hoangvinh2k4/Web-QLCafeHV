namespace QLCafeHV.Models.ViewModels
{
    public class CartViewModel
    {
        public OrderModel Order { get; set; }
        public List<CartDetailViewModel> Details { get; set; }
    }

    public class CartDetailViewModel
    {
        public int OrderDetailID { get; set; }
        public int OrderID { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public string ImageUrl { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
