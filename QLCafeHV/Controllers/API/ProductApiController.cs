using Microsoft.AspNetCore.Mvc;
using QLCafeHV.Models;


namespace QLCafeHV.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductApiController : ControllerBase
    {
        private readonly ProductService _service;

        public ProductApiController(ProductService service)
        {
            _service = service;
        }

        // ================= CREATE =================
        [HttpPost("Create")]
        public async Task<IActionResult> CreateProduct([FromForm] ProductModel product)
        {
            var result = await _service.Create(product);

            if (!result)
                return BadRequest(new { message = "Thêm thất bại" });

            return Ok(new { message = "Thêm thành công" });
        }

        // ================= UPDATE =================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] ProductModel product)
        {
            var result = await _service.Update(id, product);

            if (!result)
                return NotFound(new { message = "Không tìm thấy sản phẩm" });

            return Ok(new { message = "Cập nhật thành công" });
        }

        // ================= DELETE (SOFT) =================
        [HttpPost("Delete")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var result = await _service.Delete(id);

            if (!result)
                return NotFound(new { message = "Không tìm thấy sản phẩm" });

            return Ok(new { message = "Đã ngừng bán sản phẩm" });
        }
    }
}