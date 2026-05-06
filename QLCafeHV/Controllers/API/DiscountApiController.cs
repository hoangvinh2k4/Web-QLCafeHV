using Microsoft.AspNetCore.Mvc;
using QLCafeHV.Models;

namespace QLCafeHV.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiscountApiController : ControllerBase
    {
        private readonly DiscountService _service;

        public DiscountApiController(DiscountService service)
        {
            _service = service;
        }

        // ================= CREATE =================
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] DiscountModel model)
        {
            var result = await _service.Create(model);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
      
        // ================= UPDATE =================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] DiscountModel model)
        {
            var result = await _service.Update(id, model);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // ================= DELETE =================
        [HttpPost("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.Delete(id);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }
    }
}