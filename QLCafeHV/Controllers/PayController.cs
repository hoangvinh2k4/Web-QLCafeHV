using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using QLCafeHV;
using QLCafeHV.Helpers;
using QLCafeHV.Models;
using QLCafeHV.Models.DbConnect;
using static QLCafeHV.Models.DbConnect.VTCPayModel;

namespace QLCafeHV.Controllers
{
    [Route("Pay")]
    public class PayController : Controller
    {
        private readonly CoffeeContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;
        private readonly string urlPortalApis;
           
        public PayController(IHttpClientFactory httpClientFactory, IMemoryCache cache, CoffeeContext context)
        {
            _httpClientFactory = httpClientFactory;
            _cache = cache;
            _context = context;
            urlPortalApis = Config.AppSettings.Get("AppSettings:PortalAPIs_URL");
        }

        [HttpPost("ThanhToanTrucTuyen")]
        public async Task<IActionResult> ThanhToanTrucTuyen([FromBody] PaymentRequestModel request)
        {
            try
            {

                int orderId = request.OrderID;
                var order = _context.Orders.FirstOrDefault(o => o.OrderID == request.OrderID);
                if (order == null)
                    return Json(new PaymentURLResponseData { ResponseCode = -1, Description = "Đơn hàng không tồn tại" });
                var cacheKeyUser = $"PaymentUser_{orderId}";
                _cache.Set(cacheKeyUser, order.EmployeeID, TimeSpan.FromMinutes(30));
                // 1. Lấy cấu hình PaymentInfo
                var merchantAcc = Config.AppSettings.Get("AppSettings:PaymentInfo:MerchantAccount");
                var websiteID = Config.AppSettings.Get("AppSettings:PaymentInfo:WebsiteID");
                var secretKey = Config.AppSettings.Get("AppSettings:PaymentInfo:PublicKey");
                var returnURL = Config.AppSettings.Get("AppSettings:PaymentInfo:ReturnURL");

                // 2. Tạo mã đơn hàng
                var orderCode = DateTime.Now.ToString("yyyyMMddHHmmssffff");
                order.OrderCode = orderCode;
                _context.SaveChanges();

                // 3. Tạo signature
                var signPlainText = $"{websiteID}|{(long)request.Amount}|{merchantAcc}|{orderCode}|VND|{secretKey}";
                var signature = Encrypt.SHA256encrypt(signPlainText);
                //4.Tạo URL trả về
               var urlReturn = $"{returnURL}?smartcardserial={request.SmartcardSerial}" +
                               $"&orgTransId={orderCode}&package={request.Package}&months={request.Months}";

                // 5. Kiểm tra trùng đơn hàng bằng MemoryCache
                var cacheKey = $"Order_{orderCode}";
                if (_cache.TryGetValue(cacheKey, out _))
                {
                    return Json(new PaymentURLResponseData
                    {
                        ResponseCode = -1,
                        Description = "Lỗi trùng đơn hàng!"
                    });
                }

                // 6. Tạo object request gửi sang VTC Pay
                var requestPaymentUrl = new
                {
                    CustomerID = "",
                    Lang = "vi",
                    UrlReturn = urlReturn,
                    PaymentType = request.PayType.ToString(), 
                    WebsiteID = long.Parse(websiteID),
                    Description = $"{request.SmartcardSerial}|{orderCode}|{request.Package}|{request.Months}|{request.PromotionId}|{request.PromotionPackage}",
                    Amount = (long)request.Amount,
                    ReceiverAccount = merchantAcc,
                    OrderCode = orderCode,
                    Currency = "VND",
                    Signature = signature.ToUpper()
                };

                var apiUrl = $"{urlPortalApis}/GeneratePaymentURL";

                // 7. Gọi API VTC Pay
                var responseURL = await _httpClientFactory.PortalAPIs()
                    .PostAsync<PaymentURLResponseData>(requestPaymentUrl, apiUrl);

                if (responseURL == null || responseURL.ResponseCode <= 0 || string.IsNullOrEmpty(responseURL.PaymentUrl))
                {
                    return Json(new PaymentURLResponseData
                    {
                        ResponseCode = -1,
                        Description = "Lỗi lấy liên kết thanh toán!"
                    });
                }

                // 8. Lưu MemoryCache 15 phút chống trùng
                var keepMinutes = int.Parse(Config.AppSettings.Get("AppSettings:PaymentInfo:KeepTransactionMin") ?? "15");
                _cache.Set(cacheKey, requestPaymentUrl.Description,
                    new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(keepMinutes)));

                // 9. Trả kết quả cho JS
                return Json(responseURL);
            }
            catch (Exception ex)
            {
                return Json(new PaymentURLResponseData
                {
                    ResponseCode = -1,
                    Description = $"Lỗi thanh toán: {ex.Message}"
                });
            }
        }
       
        [HttpGet("ReturnFromVTC")]
        public IActionResult ReturnFromVTC( string orgTransId)
        {
            // 1. Lấy order
            var order = _context.Orders.FirstOrDefault(o => o.OrderCode == orgTransId);
            if (order == null)
                return Content("Không tìm thấy đơn hàng!");

            // 2. Tạo payment
            var payment = new PaymentModel
            {
                OrderID = order.OrderID,
                PaidAmount = order.TotalAmount,
                PaymentMethod = "Chuyển khoản qua cổng VTC PAY",
                PaymentTime = DateTime.Now
            };

            _context.Payments.Add(payment);

            // 3. Update trạng thái Order
            order.Status = "Đã thanh toán";

            // 4. Lưu DB
            _context.SaveChanges();

            // 6. Trả về trang user
            return RedirectToAction("Index", "Home");
        }
    }

}

