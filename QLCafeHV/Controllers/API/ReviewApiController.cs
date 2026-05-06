using Microsoft.AspNetCore.Mvc;
using QLCafeHV.Models;

namespace QLCafeHV.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewApiController : ControllerBase
    {
        private readonly ReviewService _service;

        public ReviewApiController(ReviewService service)
        {
            _service = service;
        }

        // CREATE
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] ReviewModel model)
        {
            try
            {
                var result = await _service.Create(model);

                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                // 👇 debug lỗi thật
                return StatusCode(500, new
                {
                    message = ex.Message,
                    inner = ex.InnerException?.Message,
                    stack = ex.StackTrace
                });
            }
        }

        // GET LIST
        [HttpGet("Product/{productId}")]
        public async Task<IActionResult> GetByProduct(int productId)
        {
            var data = await _service.GetByProduct(productId);
            return Ok(data);
        }
    }
}