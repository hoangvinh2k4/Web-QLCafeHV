using System.ComponentModel.DataAnnotations;

namespace QLCafeHV.Models.ViewModels
{
    public class OrderOnlineViewModel
    {
        public string OrderCode { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public decimal TotalAmount { get; set; }      
    }
    public class OrderOfflineViewModel
    {
        public string OrderId{ get; set; }
        public string TableName { get; set; }
        public decimal TotalAmount { get; set; }
    }
}