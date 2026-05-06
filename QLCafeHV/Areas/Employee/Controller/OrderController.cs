using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLCafeHV.Models;
using QLCafeHV.Models.DbConnect;

namespace QLCafeHV.Areas.Employee.Controllers
{
    [Area("Employee")]
    public class OrderController : Controller
    {
        private readonly CoffeeContext _context;

        public OrderController(CoffeeContext context)
        {
            _context = context;
        }
        public IActionResult Order(int id)
        {
            // Lấy thông tin bàn
            var table = _context.Tables.Find(id);
            if (table == null)
                return BadRequest("TableID không tồn tại!");

            // Lấy EmployeeID từ session
            var employeeId = HttpContext.Session.GetInt32("EmployeeID") ?? 0;
            if (employeeId == 0)
                return BadRequest("EmployeeID chưa được đăng nhập!");

            // Kiểm tra order Pending của bàn
            var order = _context.Orders.Where(x => x.TableID == id && x.Status == "Đang phục vụ")
                .OrderByDescending(x => x.OrderID).FirstOrDefault();
            // Nếu chưa có thì tạo mới
            if (order == null)
            {
                order = new OrderModel
                {
                    TableID = id,
                    EmployeeID = employeeId,
                    Status = "Đang phục vụ",
                    OrderTime = DateTime.Now,
                    TotalAmount = 0,
                    KitchenStatus = "Chưa làm"
                };
                _context.Orders.Add(order);
                _context.SaveChanges();
            }

            ViewBag.OrderId = order.OrderID;
            ViewBag.TableNumber = table.TableName;
            ViewBag.TableId = id;
            // Lấy danh sách món đã thêm trong Order
            var orderItems = _context.OrderDetails
                .Where(x => x.OrderID == order.OrderID)
                .Include(x => x.Product)
                .Select(x => new
                {
                    productId = x.ProductID,
                    name = x.Product.ProductName,
                    qty = x.Quantity,
                    unitprice = x.Product.Price,
                    total = x.TotalPrice,
                    note = x.Note
                })
                .ToList();
            // Tính tổng tiền từ orderItems
            order.TotalAmount = orderItems.Sum(x => x.total);

            // Lưu lại tổng tiền vào database
            _context.SaveChanges();

            // Gán vào ViewBag để hiển thị
            ViewBag.TotalAmount = order.TotalAmount;

            ViewBag.OrderItems = orderItems;

            // Lấy danh sách tất cả bàn để hiển thị trạng thái màu
            var allTables = _context.Tables.ToList();
            var tableWithOrders = _context.Orders
                .Where(o => o.Status == "Đang phục vụ")
                .Select(o => o.TableID)
                .ToList();

            foreach (var t in allTables)
            {
                if (tableWithOrders.Contains(t.TableID))
                    t.Status = "Đang phục vụ";
                else if (t.Status == "Đã đặt")
                    t.Status = "Đã đặt";
                else
                    t.Status = "Đang trống";
            }
            ViewBag.Tables = allTables; // gửi danh sách bàn sang View để đổi màu

            // Lấy danh sách sản phẩm còn bán
            var products = _context.Products
                .Where(p => p.Status == 1)
                .OrderBy(p => p.ProductName)
                .ToList();

            return View(products);
        }

        [HttpPost]
        public IActionResult AddItem(int orderId, int productId)
        {
            var order = _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(d => d.Product)
                .FirstOrDefault(o => o.OrderID == orderId);

            if (order == null) return BadRequest("Order không tồn tại!");

            var product = _context.Products.Find(productId);
            if (product == null) return BadRequest("Sản phẩm không tồn tại!");

            order.OrderType = "Offline";
            // ✅ chỉ set khi cần
            if (string.IsNullOrEmpty(order.KitchenStatus) || order.KitchenStatus == "Hoàn thành")
            {
                order.KitchenStatus = "Chưa làm";
            }

            if (order.Status != "Đang phục vụ")
                order.Status = "Đang phục vụ";

            var table = _context.Tables.FirstOrDefault(t => t.TableID == order.TableID);
            if (table != null)
                table.Status = "Đang phục vụ";

            var detail = order.OrderDetails
                .FirstOrDefault(x => x.ProductID == productId);

            if (detail == null)
            {
                detail = new OrderDetailModel
                {
                    OrderID = orderId,
                    ProductID = productId,
                    Quantity = 1,
                    UnitPrice = product.Price,
                    TotalPrice = product.Price
                };

                _context.OrderDetails.Add(detail);
            }
            else
            {
                detail.Quantity++;
                detail.TotalPrice = detail.Quantity * detail.UnitPrice;
            }

            order.TotalAmount = order.OrderDetails.Sum(x => x.TotalPrice);

            _context.SaveChanges();

            var items = order.OrderDetails
                .Select(x => new
                {
                    productId = x.ProductID,
                    name = x.Product != null ? x.Product.ProductName : "",
                    qty = x.Quantity,
                    unitprice = x.UnitPrice,
                    total = x.TotalPrice,
                    note = x.Note
                }).ToList();

            return Json(items);
        }

        [HttpPost]
        public IActionResult RemoveItem(int orderId, int productId)
        {
            // 1. Lấy chi tiết cần xóa
            var detail = _context.OrderDetails
                .FirstOrDefault(x => x.OrderID == orderId && x.ProductID == productId);

            if (detail == null)
                return BadRequest("Món không tồn tại trong Order!");

            // 2. Xóa item
            _context.OrderDetails.Remove(detail);
            _context.SaveChanges();

            // 3. Tính lại tổng tiền (ToList trước)
            var totalAmount = _context.OrderDetails
                .Where(x => x.OrderID == orderId)
                .Select(x => x.TotalPrice)
                .ToList()
                .Sum();

            var order = _context.Orders.Find(orderId);
            if (order != null)
            {
                order.TotalAmount = totalAmount;
                _context.SaveChanges();
            }

            // 4. Lấy lại danh sách item để trả về JS
            var items = _context.OrderDetails
                .Where(x => x.OrderID == orderId)
                .Include(x => x.Product)
                .Select(x => new
                {
                    productId = x.ProductID,
                    name = x.Product.ProductName,
                    qty = x.Quantity,
                    unitprice = x.Product.Price,
                    total = x.TotalPrice,
                    note = x.Note
                })
                .ToList();
            return Json(items);
        }

        [HttpPost]
        public IActionResult CancelOrder(int orderId)
        {
            var order = _context.Orders
                                .Include(o => o.OrderDetails)
                                .FirstOrDefault(o => o.OrderID == orderId);

            if (order == null)
            {
                return Json(new { success = false, message = "Order không tồn tại!" });
            }
          
            order.Status = "Đã hủy";
         
            var table = _context.Tables.FirstOrDefault(t => t.TableID == order.TableID);
            if (table != null)
            {
                table.Status = "Đang trống";
            }

            _context.SaveChanges();

            return Json(new { success = true, message = "Hủy đơn hàng thành công!" });
        }

        [HttpPost]
        public IActionResult UpdateNote(int orderId, int productId, string note)
        {
            var detail = _context.OrderDetails
                .FirstOrDefault(x => x.OrderID == orderId && x.ProductID == productId);

            if (detail == null)
                return BadRequest("Không tìm thấy món!");

            detail.Note = note;
            _context.SaveChanges();

            // Trả lại danh sách món
            var items = _context.OrderDetails
                .Where(x => x.OrderID == orderId)
                .Include(x => x.Product)
                .Select(x => new
                {
                    productId = x.ProductID,
                    name = x.Product.ProductName,
                    qty = x.Quantity,
                    unitprice = x.Product.Price,
                    total = x.TotalPrice,
                    note = x.Note
                }).ToList();

            return Json(items);
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int orderId, int productId, int quantity)
        {
            var detail = _context.OrderDetails.FirstOrDefault(x => x.OrderID == orderId && x.ProductID == productId);
            if (detail == null) return BadRequest("Món không tồn tại trong Order!");

            detail.Quantity = quantity;
            detail.TotalPrice = quantity * detail.UnitPrice;
            _context.SaveChanges();

            // Cập nhật tổng tiền Order
            var order = _context.Orders.Find(orderId);
            if (order != null)
            {
                order.TotalAmount = _context.OrderDetails.Where(x => x.OrderID == orderId).Sum(x => x.TotalPrice);
                _context.SaveChanges();
            }

            // Trả dữ liệu JSON
            var items = _context.OrderDetails
                .Where(x => x.OrderID == orderId)
                .Include(x => x.Product)
                .Select(x => new
                {
                    productId = x.ProductID,
                    name = x.Product.ProductName,
                    qty = x.Quantity,
                    unitprice = x.UnitPrice,
                    total = x.TotalPrice,
                    note = x.Note
                }).ToList();

            return Json(items);
        }

        [HttpPost]
        public IActionResult ApplyDiscount(int orderId, string code)
        {
            var order = _context.Orders.Find(orderId);

            if (order == null)
                return Json(new { success = false, message = "Không tìm thấy đơn" });

            var discount = _context.Discounts
                .FirstOrDefault(x =>
                    x.Code == code &&
                    x.Status == 1 &&
                    x.Quantity > 0 &&
                    x.StartDate <= DateTime.Now &&
                    x.EndDate >= DateTime.Now);

            if (discount == null)
                return Json(new { success = false, message = "Mã không hợp lệ hoặc hết hạn" });

            decimal discountAmount = order.TotalAmount * discount.PercentValue / 100m;

            order.TotalAmount -= discountAmount;

            _context.SaveChanges();

            return Json(new
            {
                success = true,
                message = $"Giảm {discount.PercentValue}%",
                newTotal = order.TotalAmount.ToString("N0")
            });
        }

        [HttpPost]
        public IActionResult PayOrder(int orderId, string paymentMethod, string discountCode)
        {
            var order = _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefault(o => o.OrderID == orderId);

            if (order == null)
                return Json(new { success = false, message = "Order không tồn tại!" });

            if (order.OrderDetails == null || !order.OrderDetails.Any())
                return Json(new { success = false, message = "Đơn hàng chưa có sản phẩm, không thể thanh toán!" });

            var table = _context.Tables.FirstOrDefault(t => t.TableID == order.TableID);
            if (table != null)
            {
                table.Status = "Đang trống";
            }

            try
            {
                decimal totalAmount = order.OrderDetails.Sum(d => d.TotalPrice);

                // Nếu có mã giảm giá thì tính lại
                if (!string.IsNullOrEmpty(discountCode))
                {
                    var discount = _context.Discounts.FirstOrDefault(x =>
                        x.Code == discountCode &&
                        x.Status == 1 &&
                        x.Quantity > 0);

                    if (discount != null)
                    {
                        decimal discountAmount = totalAmount * discount.PercentValue / 100m;
                        totalAmount -= discountAmount;

                        // Chỉ thanh toán mới trừ mã
                        discount.Quantity -= 1;
                    }
                }

                // Lưu Payment
                var payment = new PaymentModel
                {
                    OrderID = order.OrderID,
                    PaymentMethod = paymentMethod,
                    PaidAmount = totalAmount,
                    PaymentTime = DateTime.Now
                };

                _context.Payments.Add(payment);

                // Update Order
                order.Status = "Đã thanh toán";
                order.CheckOutTime = DateTime.Now;
                order.TotalAmount = totalAmount;

                _context.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetShiftSummary()
        {
            int employeeId = HttpContext.Session.GetInt32("EmployeeID") ?? 0;

            var today = DateTime.Today;

            var shift = _context.EWorkShifts 
             .FirstOrDefault(x =>x.EmployeeID == employeeId &&x.Status == "Opened");

            if (shift == null)
                return Json(new { success = false, message = "Chưa mở ca" });

            decimal totalSales = _context.Payments
                .Where(x => x.PaymentTime.Date == today)
                .Sum(x => (decimal?)x.PaidAmount) ?? 0;

            decimal expected = shift.OpenAmount + totalSales;

            decimal difference = shift.CloseAmount.HasValue
                ? shift.CloseAmount.Value - expected
                : 0;

            return Json(new
            {
                success = true,
                openAmount = shift.OpenAmount,
                totalSales = totalSales,
                expected = expected,
                closeAmount = shift.CloseAmount,
                difference = difference
            });
        }
    }
}

