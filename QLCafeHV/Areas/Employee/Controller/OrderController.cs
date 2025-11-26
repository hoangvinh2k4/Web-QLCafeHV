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
            var order = _context.Orders
                .FirstOrDefault(x => x.TableID == id && x.Status == "Đang phục vụ");

            // Nếu chưa có thì tạo mới
            if (order == null)
            {
                order = new OrderModel
                {
                    TableID = id,
                    EmployeeID = employeeId,
                    Status = "Đang phục vụ",
                    OrderTime = DateTime.Now,
                    TotalAmount = 0
                };
                _context.Orders.Add(order);
                _context.SaveChanges();
            }

            ViewBag.OrderId = order.OrderID;

            // Lấy danh sách món đã thêm trong Order
            var orderItems = _context.OrderDetails
                .Where(x => x.OrderID == order.OrderID)
                .Include(x => x.Product)
                .Select(x => new
                {
                    productId = x.ProductID,
                    name = x.Product.ProductName,
                    qty = x.Quantity,
                    price = x.Product.Price,
                    total = x.TotalPrice
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
                .Where(p => p.Status == "Còn bán")
                .OrderBy(p => p.ProductName)
                .ToList();

            return View(products);
        }
       
        [HttpPost]
        public IActionResult AddItem(int orderId, int productId)
        {
            // Load order + details + product
            var order = _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(d => d.Product)
                .FirstOrDefault(o => o.OrderID == orderId);

            if (order == null) return BadRequest("Order không tồn tại!");

            var product = _context.Products.Find(productId);
            if (product == null) return BadRequest("Sản phẩm không tồn tại!");

            // Đổi trạng thái phục vụ
            if (order.Status != "Đang phục vụ")
                order.Status = "Đang phục vụ";

            var table = _context.Tables.FirstOrDefault(t => t.TableID == order.TableID);
            if (table != null)
                table.Status = "Đang phục vụ";

            // Kiểm tra món trong order
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

            _context.SaveChanges();

            // Cập nhật tổng tiền order
            order.TotalAmount = order.OrderDetails.Sum(x => x.TotalPrice);
            _context.SaveChanges();

            // Trả danh sách mới cho JS
            var items = order.OrderDetails
                .Select(x => new
                {
                    productId = x.ProductID,
                    name = x.Product.ProductName,
                    qty = x.Quantity,
                    unitprice = x.UnitPrice,
                    total = x.TotalPrice
                }).ToList();

            return Json(items);
        }
        
        [HttpPost]
        public IActionResult RemoveItem(int orderId,int productId)
        {
            var detail = _context.OrderDetails
                .FirstOrDefault(x => x.OrderID == orderId && x.ProductID == productId);
            if (detail == null)
                return BadRequest("Món không tồn tại trong Order!");
            _context.OrderDetails.Remove(detail);
            _context.SaveChanges();
            // Cập nhật tổng tiền
            var order = _context.Orders.Find(orderId);
            if (order != null)
            {
                order.TotalAmount = _context.OrderDetails
                    .Where(x => x.OrderID == orderId)
                    .Sum(x => x.TotalPrice);
                _context.SaveChanges();
            }
            // Trả dữ liệu JSON cho JS
            var items = _context.OrderDetails
                .Where(x => x.OrderID == orderId)
                .Include(x => x.Product)
                .Select(x => new
                {
                    productId = x.ProductID,
                    name = x.Product.ProductName,
                    qty = x.Quantity,
                    unitPrice = x.Product.Price,
                    total = x.TotalPrice
                }).ToList();
            return Json(items);
        }

        [HttpPost]
        public IActionResult CancelOrder(int orderId)
        {
            var order = _context.Orders.Include(o => o.OrderDetails).FirstOrDefault(o => o.OrderID == orderId);
            if(order == null)
            {
                return Json(new { success = false, message = "Order không tồn tại!" });
            }
            var table = _context.Tables.FirstOrDefault(t => t.TableID == order.TableID);
            if (table != null)
            {
                table.Status = "Đang trống";
            }
            try
            {
                if(order.OrderDetails != null && order.OrderDetails.Any())
                {
                    _context.OrderDetails.RemoveRange(order.OrderDetails);
                }
                _context.Orders.Remove(order);
                _context.SaveChanges();
                return Json(new { success = true, message = "Hủy đơn hàng thành công !" });
            }catch(Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
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
                    total = x.TotalPrice
                }).ToList();

            return Json(items);
        }
        
        [HttpPost]
        public IActionResult PayOrder(int orderId, string paymentMethod)
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
             
                // Lưu Payment
                var payment = new PaymentModel
                {
                    OrderID = order.OrderID,
                    PaymentMethod = paymentMethod,
                    PaidAmount = totalAmount,
                    PaymentTime = DateTime.Now
                };
                _context.Payments.Add(payment);

                // Cập nhật Order
                order.Status = "Đã thanh toán";
                order.CheckOutTime = DateTime.Now;

                _context.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}

