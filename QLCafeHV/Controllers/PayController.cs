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
                if (request == null || request.OrderID <= 0)
                {
                    return Json(new PaymentURLResponseData
                    {
                        ResponseCode = -1,
                        Description = "Dữ liệu không hợp lệ"
                    });
                }

                int orderId = request.OrderID;

                // 🔥 LẤY ORDER + DETAILS
                var order = _context.Orders
                    .Include(o => o.OrderDetails)
                    .FirstOrDefault(o => o.OrderID == orderId);

                if (order == null)
                    return Json(new PaymentURLResponseData { ResponseCode = -1, Description = "Đơn hàng không tồn tại" });

                var cacheKeyUser = $"PaymentUser_{orderId}";
                _cache.Set(cacheKeyUser, order.EmployeeID, TimeSpan.FromMinutes(30));

                // ================== 🔥 TÍNH TIỀN ==================
                var subtotal = order.OrderDetails.Sum(x => x.UnitPrice * x.Quantity);

                decimal shipping = request.District switch
                {
                    "BaDinh" => 15000,
                    "HoanKiem" => 15000,
                    "DongDa" => 18000,
                    "HaiBaTrung" => 18000,
                    "CauGiay" => 20000,
                    "ThanhXuan" => 20000,
                    "HoangMai" => 22000,
                    "LongBien" => 25000,
                    "HaDong" => 30000,
                    "NamTuLiem" => 25000,
                    "BacTuLiem" => 25000,
                    _ => 0
                };

                decimal discount = 0;              
                var total = subtotal + shipping - discount;

                // (optional) lưu lại DB
                order.TotalAmount = total;
                // =================================================

                // 1. Lấy cấu hình
                var merchantAcc = Config.AppSettings.Get("AppSettings:PaymentInfo:MerchantAccount");
                var websiteID = Config.AppSettings.Get("AppSettings:PaymentInfo:WebsiteID");
                var secretKey = Config.AppSettings.Get("AppSettings:PaymentInfo:PublicKey");
                var returnURL = Config.AppSettings.Get("AppSettings:PaymentInfo:ReturnURL");

                // 2. Tạo mã đơn hàng
                var orderCode = DateTime.Now.ToString("yyyyMMddHHmmssffff");
                order.OrderCode = orderCode;
                _context.SaveChanges();

                var deliveryCacheKey = $"Delivery_{orderCode}";

                var deliveryInfo = new OrderDeliveryModel
                {
                    OrderID = order.OrderID,
                    FullName = request.FullName,
                    Phone = request.Phone,
                    Address = request.Address,
                    District = request.District,
                    ShippingFee = shipping, // bạn đã tính ở backend

                };

                _cache.Set(deliveryCacheKey, deliveryInfo, TimeSpan.FromMinutes(30));

                // ================== 🔥 SỬA SIGNATURE ==================
                var signPlainText = $"{websiteID}|{(long)total}|{merchantAcc}|{orderCode}|VND|{secretKey}";
                var signature = Encrypt.SHA256encrypt(signPlainText);
                // =====================================================

                // 4. URL return
                var urlReturn = $"{returnURL}?smartcardserial={request.SmartcardSerial}" +
                                $"&orgTransId={orderCode}&package={request.Package}&months={request.Months}";

                // 5. Check trùng
                var cacheKey = $"Order_{orderCode}";
                if (_cache.TryGetValue(cacheKey, out _))
                {
                    return Json(new PaymentURLResponseData
                    {
                        ResponseCode = -1,
                        Description = "Lỗi trùng đơn hàng!"
                    });
                }

                // ================== 🔥 REQUEST VTC ==================
                var requestPaymentUrl = new
                {
                    CustomerID = "",
                    Lang = "vi",
                    UrlReturn = urlReturn,
                    PaymentType = request.PayType,
                    WebsiteID = long.Parse(websiteID),
                    Description = $"{request.SmartcardSerial}|{orderCode}|{request.Package}|{request.Months}|{request.PromotionId}|{request.PromotionPackage}",
                    Amount = (long)total,   // 🔥 dùng total backend
                    ReceiverAccount = merchantAcc,
                    OrderCode = orderCode,
                    Currency = "VND",
                    Signature = signature.ToUpper()
                };
                // ====================================================

                var apiUrl = $"{urlPortalApis}/GeneratePaymentURL";

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

                var keepMinutes = int.Parse(Config.AppSettings.Get("AppSettings:PaymentInfo:KeepTransactionMin") ?? "15");
                _cache.Set(cacheKey, requestPaymentUrl.Description,
                    new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(keepMinutes)));

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
        public IActionResult ReturnFromVTC(string orgTransId)
        {
            // 1. Lấy order
            var order = _context.Orders.FirstOrDefault(o => o.OrderCode == orgTransId);
            if (order == null)
                return Content("Không tìm thấy đơn hàng!");

            // 2. Lấy delivery từ cache
            var deliveryCacheKey = $"Delivery_{orgTransId}";
            if (!_cache.TryGetValue(deliveryCacheKey, out OrderDeliveryModel delivery))
            {
                return Content("Không tìm thấy thông tin giao hàng!");
            }

            // 3. Lưu Delivery
            var newDelivery = new OrderDeliveryModel
            {
                OrderID = order.OrderID,
                FullName = delivery.FullName,
                Phone = delivery.Phone,
                Address = delivery.Address,
                District = delivery.District,
                ShippingFee = delivery.ShippingFee,
            };

            _context.Add(newDelivery);

            var subtotal = _context.OrderDetails.Where(x => x.OrderID == order.OrderID).Sum(x => x.TotalPrice);
            // 4. Tạo payment
            var payment = new PaymentModel
            {
                OrderID = order.OrderID,
                PaidAmount = subtotal,
                PaymentMethod = "Chuyển khoản qua cổng VTC PAY",
                PaymentTime = DateTime.Now
            };

            _context.Payments.Add(payment);

            // 5. Update trạng thái Order
            order.Status = "Đã thanh toán";

            // 6. Lưu DB
            _context.SaveChanges();

            // 7. Xóa cache
            _cache.Remove(deliveryCacheKey);

            return RedirectToAction("Index", "Home");
        }
    }

}

