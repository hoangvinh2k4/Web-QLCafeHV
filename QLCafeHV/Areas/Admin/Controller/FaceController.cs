using Microsoft.AspNetCore.Mvc;
using QLCafeHV.Models;
using QLCafeHV.Models.DbConnect;
using QLCafeHV.Models.ViewModels;
using System.Text.Json;

namespace QLCafeHV.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class FaceController : Controller
    {
        private readonly CoffeeContext _context;

        public FaceController(CoffeeContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult RegisterFace()
        {
            var employees = _context.Employees.ToList();
            return View(employees);
        }

        [HttpPost]
        public async Task<IActionResult> RegisterFace([FromBody] FaceRegisterRequestModel request)
        {
            try
            {
                using var client = new HttpClient();

                var response = await client.PostAsJsonAsync(
                    "http://127.0.0.1:5000/register-face",
                    request
                );

                var json = await response.Content.ReadAsStringAsync();

                var flaskResult = JsonSerializer.Deserialize<VerifyFaceResponseModel>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (flaskResult != null && flaskResult.success)
                {
                    var employee = _context.Employees.FirstOrDefault(x => x.EmployeeID == request.EmployeeId
                       && x.Role == "Employee");

                    if (employee != null)
                    {
                        employee.FaceRegistered = true;
                        employee.FaceImagePath = $"/facesregister/{request.EmployeeId}.jpg";

                        _context.SaveChanges();
                    }

                    return Json(new
                    {
                        success = true,
                        message = flaskResult.message
                    });
                }

                return Json(new
                {
                    success = false,
                    message = flaskResult?.message ?? "Đăng ký thất bại"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}