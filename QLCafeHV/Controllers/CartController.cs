using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.X509;
using QLCafeHV.Models;
using QLCafeHV.Models.DbConnect;
using QLCafeHV.Models.ViewModels;

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

            var order = _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefault(o => o.EmployeeID == userId && o.Status == "Pending");

            // 🔥 nếu không có đơn → giỏ trống
            if (order == null)
            {
                return View(new CartViewModel
                {
                    Order = null,
                    Details = new List<CartDetailViewModel>()
                });
            }

            // update total
            if (order.OrderDetails != null && order.OrderDetails.Any())
            {
                order.TotalAmount = order.OrderDetails.Sum(x => x.TotalPrice);
                _context.SaveChanges();
            }

            var model = new CartViewModel
            {
                Order = order,
                Details = order.OrderDetails.Select(x => new CartDetailViewModel
                {
                    OrderDetailID = x.OrderDetailID,
                    OrderID = x.OrderID,
                    ProductName = x.Product?.ProductName,
                    ImageUrl = x.Product?.ImageUrl,
                    UnitPrice = x.UnitPrice,
                    CategoryName = x.Product?.Category,
                    Quantity = x.Quantity,
                    TotalPrice = x.TotalPrice
                }).ToList()
            };

            return View(model);
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
                    OrderType="Online",
                    OrderDetails = new List<OrderDetailModel>()
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
                return Json(new { success = false, message = "Bạn chưa đăng nhập" });

            var detail = _context.OrderDetails
                .Include(x => x.Order)
                .FirstOrDefault(x => x.OrderDetailID == orderDetailId);

            if (detail == null || detail.Order.EmployeeID != userId)
                return Json(new { success = false, message = "Không tìm thấy sản phẩm" });

            var order = detail.Order;

            _context.OrderDetails.Remove(detail);
            _context.SaveChanges();

            // cập nhật tổng tiền
            var total = _context.OrderDetails
                .Where(x => x.OrderID == order.OrderID)
                .Sum(x => (decimal?)x.TotalPrice) ?? 0;

            order.TotalAmount = total;
            _context.SaveChanges();

            return Json(new
            {
                success = true,
                totalAmount = total
            });
        }

        [HttpPost]
        public IActionResult IncreaseQuantity(int id)
        {
            var detail = _context.OrderDetails.FirstOrDefault(x => x.OrderDetailID == id);
            if (detail == null)
                return Json(new { success = false });

            detail.Quantity += 1;
            detail.TotalPrice = detail.Quantity * detail.UnitPrice;

            _context.SaveChanges();

            return Json(new
            {
                success = true,
                quantity = detail.Quantity,
                totalPrice = detail.TotalPrice
            });
        }
        [HttpPost]
        public IActionResult DecreaseQuantity(int id)
        {
            var detail = _context.OrderDetails.FirstOrDefault(x => x.OrderDetailID == id);
            if (detail == null)
                return Json(new { success = false });

            // ❌ Nếu đang là 1 thì không cho giảm nữa
            if (detail.Quantity <= 1)
            {
                return Json(new
                {
                    success = false,
                    message = "Số lượng tối thiểu là 1"
                });
            }

            // ✅ Giảm bình thường
            detail.Quantity--;
            detail.TotalPrice = detail.Quantity * detail.UnitPrice;

            _context.SaveChanges();

            return Json(new
            {
                success = true,
                quantity = detail.Quantity,
                totalPrice = detail.TotalPrice
            });
        }
        [HttpPost]
        public IActionResult UpdateQuantity([FromBody] UpdateQuantityRequest req)
        {
            var detail = _context.OrderDetails.FirstOrDefault(x => x.OrderDetailID == req.Id);
            if (detail == null)
                return Json(new { success = false });

            // ❌ chặn nhỏ hơn 1
            if (req.Quantity < 1)
                req.Quantity = 1;

            detail.Quantity = req.Quantity;
            detail.TotalPrice = detail.Quantity * detail.UnitPrice;

            _context.SaveChanges();

            return Json(new
            {
                success = true,
                quantity = detail.Quantity,
                totalPrice = detail.TotalPrice
            });
        }

        public class UpdateQuantityRequest
        {
            public int Id { get; set; }
            public int Quantity { get; set; }
        }
    }
}
