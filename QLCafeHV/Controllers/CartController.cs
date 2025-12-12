using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLCafeHV.Models;
using QLCafeHV.Models.DbConnect;

namespace QLCafeHV.Controllers
{
    public class CartController : Controller
    {
        private readonly CoffeeContext _context;
        public CartController(CoffeeContext context)
        {
            _context = context;
        }
        public IActionResult Cart()
        {
            var userId = HttpContext.Session.GetInt32("EmployeeID");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            // Lấy đơn hàng đang mở của user
            var order = _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product) // navigation property Product
                .FirstOrDefault(o => o.EmployeeID == userId && o.Status == "Pending");

            if (order != null && order.OrderDetails != null)
            {
                // Cập nhật lại tổng tiền
                order.TotalAmount = order.OrderDetails.Sum(od => od.TotalPrice);
                _context.SaveChanges();
            }

            return View(order);
        }
        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity = 1)
        {
            // Lấy UserID từ session
            var userId = HttpContext.Session.GetInt32("EmployeeID");
            if (userId == null)
                return Json(new { success = false, message = "Bạn cần đăng nhập" });

            // Lấy sản phẩm từ DB
            var product = _context.Products.FirstOrDefault(p => p.ProductID == productId);
            if (product == null)
                return Json(new { success = false, message = "Sản phẩm không tồn tại" });

            // Lấy đơn hàng đang mở của user, include OrderDetails
            var order = _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefault(o => o.EmployeeID == userId && o.Status == "Pending");

            // Nếu chưa có đơn hàng, tạo mới
            if (order == null)
            {
                order = new OrderModel
                {
                    EmployeeID = userId.Value,
                    OrderTime = DateTime.Now,
                    Status = "Pending",
                    TotalAmount = 0,
                    OrderDetails = new List<OrderDetailModel>() // khởi tạo collection
                };
                _context.Orders.Add(order);
                _context.SaveChanges();
            }
            else if (order.OrderDetails == null)
            {
                order.OrderDetails = new List<OrderDetailModel>();
            }

            // Kiểm tra sản phẩm đã có trong OrderDetails chưa
            var orderDetail = order.OrderDetails.FirstOrDefault(od => od.ProductID == productId);
            if (orderDetail != null)
            {
                // Nếu có thì tăng số lượng
                orderDetail.Quantity += quantity;
                orderDetail.TotalPrice = orderDetail.Quantity * orderDetail.UnitPrice;
            }
            else
            {
                // Nếu chưa có thì thêm mới
                orderDetail = new OrderDetailModel
                {
                    OrderID = order.OrderID,
                    ProductID = product.ProductID,
                    Quantity = quantity,
                    UnitPrice = product.Price,
                    TotalPrice = product.Price * quantity
                };

                order.OrderDetails.Add(orderDetail);
                _context.OrderDetails.Add(orderDetail);
            }

            // Cập nhật tổng tiền
            order.TotalAmount = order.OrderDetails.Sum(od => od.TotalPrice);

            _context.SaveChanges();

            return Json(new { success = true, message = $"Đã thêm {product.ProductName} vào giỏ hàng!" });
        }
        [HttpPost]
        public IActionResult RemoveItem(int orderDetailId)
        {
            var userId = HttpContext.Session.GetInt32("EmployeeID");
            if (userId == null)
                return Json(new { success = false, message = "Bạn cần đăng nhập" });

            var orderDetail = _context.OrderDetails
                .Include(od => od.Order)
                .FirstOrDefault(od => od.OrderDetailID == orderDetailId);

            if (orderDetail == null || orderDetail.Order.EmployeeID != userId)
                return Json(new { success = false, message = "Sản phẩm không tồn tại trong giỏ hàng" });

            var order = orderDetail.Order;

            _context.OrderDetails.Remove(orderDetail);

            // Cập nhật lại tổng tiền
            order.TotalAmount = order.OrderDetails
                .Where(od => od.OrderDetailID != orderDetailId)
                .Sum(od => od.TotalPrice);

            _context.SaveChanges();

            return Json(new { success = true, message = "Sản phẩm đã được xóa khỏi giỏ hàng", totalAmount = order.TotalAmount });
        }

    }
}
