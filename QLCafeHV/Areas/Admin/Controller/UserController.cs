using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLCafeHV.Models;
using QLCafeHV.Models.DbConnect;
using QLCafeHV.Models.ViewModels;

namespace QLCafeHV.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserController : Controller
    {
        private readonly CoffeeContext _context;
        public UserController(CoffeeContext context)
        {
            _context = context;
        }
        public IActionResult Index(string keyword , int page = 1)
        {
            int pageSize = 5;
            var data = _context.Employees.Include(e => e.Account).Where(e => e.Role == "User" &&e.Status == 1);
            if (!string.IsNullOrEmpty(keyword))
            {
                data = data.Where(e=>e.EmployeeID.ToString().Contains(keyword)||
                e.FullName.Contains(keyword)||e.Account.Username.Contains(keyword));
            }
            int totalItems = data.Count();
            var items = data
                .OrderBy(e => e.EmployeeID)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new UserAccountViewModel
                {
                    Employee = new EmployeeModel
                    {
                        EmployeeID = e.EmployeeID,
                        FullName = e.FullName,
                        Phone = e.Phone,
                        Address = e.Address,
                        Cccd = e.Cccd,
                        Role = e.Role
                    },
                    Account = new AccountModel
                    {
                        Username = e.Account.Username,
                        PasswordHash = e.Account.PasswordHash,
                        Role = e.Account.Role
                    }
                }).ToList();
            var model = new PageViewModel<UserAccountViewModel>
            {
                Items = items,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            };

            ViewBag.Keyword = keyword;

            return View(model);
        }
    }
}
