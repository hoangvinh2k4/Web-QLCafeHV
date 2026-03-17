using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLCafeHV.Models;
using QLCafeHV.Models.DbConnect;
using QLCafeHV.Models.ViewModels;

namespace QLCafeHV.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderController : Controller
    {
        private readonly CoffeeContext _context;
        public OrderController(CoffeeContext context)
        {
            _context = context;
        }
        public IActionResult Paid(string keyword, int page = 1)
        {
            int pageSize = 5;

            var query = _context.Payments.AsQueryable();

            // --- Lọc theo từ khoá ---
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(p =>
                    p.PaymentID.ToString().Contains(keyword) ||
                    p.OrderID.ToString().Contains(keyword) ||
                    p.PaymentMethod.Contains(keyword));
            }

            // --- Tổng số item sau khi lọc ---
            int totalItems = query.Count();

            // --- Lấy dữ liệu phân trang ---
            var data = query
                .OrderByDescending(p => p.PaymentID)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var model = new PageViewModel<PaymentModel>
            {
                Items = data,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            };

            ViewBag.Keyword = keyword;
            return View(model);
        }
        public IActionResult Serving(string keyword, int page = 1)
        {
            int pageSize = 5;

            var orders = _context.Orders
                .Include(o => o.Table)
                .Include(o => o.Employee)
                .Where(o => o.Status != null && o.Status.Contains("Đang phục vụ"))
                .Select(o => new OrderDetailsViewModel
                {
                    OrderID = o.OrderID,
                    OrderTime = o.OrderTime,
                    TotalAmount = o.TotalAmount,
                    EmployeeName = o.Employee.FullName,
                    TableName = o.Table != null ? o.Table.TableName : "Online / Mang về",
                    Items = o.OrderDetails.Select(od => new ProductOrderDetailsViewModel
                    {
                        ProductName = od.Product.ProductName,
                        Category = od.Product.Category,
                        Price = od.UnitPrice,
                        Quantity = od.Quantity
                    }).ToList()
                })
                .ToList();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();

                orders = orders.Where(o =>
                    o.OrderID.ToString().Contains(keyword)
                    || o.TableName.Contains(keyword)).ToList();
            }

            int totalItems = orders.Count();

            var data = orders
                .OrderByDescending(o => o.OrderID)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var model = new PageViewModel<OrderDetailsViewModel>
            {
                Items = data,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            };

            ViewBag.Keyword = keyword;
            return View(model);
        }

        public IActionResult Cancel(string keyword, int page = 1)
        {
            int pageSize = 5;
            var orders = _context.Orders
                .Include(o => o.Table)
                .Include(o => o.Employee)
                .Where(o => o.Status != null && o.Status.Contains("Đã hủy"))
                .Select(o => new OrderDetailsViewModel
                {
                    OrderID = o.OrderID,
                    OrderTime = o.OrderTime,
                    TotalAmount = o.TotalAmount,
                    EmployeeName = o.Employee.FullName,
                    TableName = o.Table != null ? o.Table.TableName : "Online / Mang về",
                    Items = o.OrderDetails.Select(od => new ProductOrderDetailsViewModel
                    {
                        ProductName = od.Product.ProductName,
                        Category = od.Product.Category,
                        Price = od.UnitPrice,
                        Quantity = od.Quantity
                    }).ToList()
                })
                .ToList();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                orders = orders.Where(o=>o.OrderID.ToString().Contains(keyword)||
                o.TableName.Contains(keyword)).ToList();
            }
            int totalItems = orders.Count();
            var data = orders.OrderByDescending(o => o.OrderID)
                .Skip((page - 1)*pageSize)
                .Take(pageSize)
                .ToList();
            var model = new PageViewModel<OrderDetailsViewModel>
            {
                Items = data,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            };
            ViewBag.Keyword = keyword;
            return View(model);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var payment = _context.Payments.Find(id);
            if (payment == null)
            {
                return Json(new { success = false, message = "Đơn hàng không tồn tại!" });
            }
            
            _context.Payments.Remove(payment);
            _context.SaveChanges();
            return Json(new { success = true, message = "Xóa đơn hàng thành công!" });
        }
        public IActionResult GetPaymentDetail(int id)
        {
            var payment = _context.Payments
                                  .FirstOrDefault(x => x.PaymentID == id);

            if (payment == null)
                return Json(new { success = false });

            var order = _context.Orders
                                .Include(o => o.OrderDetails)
                                .ThenInclude(od => od.Product)     
                                .FirstOrDefault(o => o.OrderID == payment.OrderID);

            if (order == null)
                return Json(new { success = false });

            var product = order.OrderDetails.Select(od => new ProductOrderDetailsViewModel
            {
                ProductName = od.Product.ProductName,
                Category = od.Product.Category,
                Price = od.UnitPrice,
                Quantity = od.Quantity
            }).ToList();

            return Json(new
            {
                success = true,
                payment = new
                {
                    id = payment.PaymentID,
                    amount = payment.PaidAmount,
                    time = payment.PaymentTime.ToString("dd/MM/yyyy HH:mm"),
                    method = payment.PaymentMethod
                },
                products = product
            });
        }
    }
}
