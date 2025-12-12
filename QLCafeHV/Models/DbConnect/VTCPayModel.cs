namespace QLCafeHV.Models.DbConnect
{
    public class VTCPayModel
    {
        public class PaymentRequestModel
        {
            public int OrderID { get; set; }
            public decimal Amount { get; set; }
            public string SmartcardSerial { get; set; }
            public string Package { get; set; }
            public int Months { get; set; }
            public string PayType { get; set; } 
            public string? PromotionId { get; set; }
            public string? PromotionPackage { get; set; }
        }

        public class PaymentURLResponseData
        {
            public int ResponseCode { get; set; }
            public string Description { get; set; }
            public string PaymentUrl { get; set; }
        }

    }
}
