using Microsoft.AspNetCore.Mvc;
using QLCafeHV.Models;

namespace QLCafeHV.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class TableApiController : ControllerBase
    {
        private readonly TableService _service;

        public TableApiController(TableService service)
        {
            _service = service;
        }

        // ================= CREATE =================
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] TableModel model)
        {
            var result = await _service.Create(model);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TableModel model)
        {
            var result = await _service.Update(id, model);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.Delete(id);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }
    }
}