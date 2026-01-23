using Application.Monsters;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MonsterController : ControllerBase
    {
        private readonly IMonsterService _service;

        public MonsterController(IMonsterService service)
        {
            _service = service;
        }
        [HttpGet]
        public async Task<ActionResult<List<MonsterDto>>> GetAll(CancellationToken ct)
        {
            var monsters = await _service.GetAllAsync(ct);
            return Ok(monsters);
        }
        [HttpGet("{id:int}")]
        public async Task<ActionResult<MonsterDto>> GetById(int id, CancellationToken ct)
        {
            var monster = await _service.GetByIdAsync(id, ct);
            if (monster is null)
                return NotFound($"Monster {id} not found.");

            return Ok(monster);
        }
        [HttpPost]
        public async Task<ActionResult<int>> Create([FromBody] CreateMonsterRequest request, CancellationToken ct)
        {
            var id = await _service.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateMonsterRequest request, CancellationToken ct)
        {
            if (id != request.Id)
                return BadRequest("ID mismatch between route and body.");

            await _service.UpdateAsync(request, ct);
            return NoContent();
        }
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            await _service.DeleteAsync(id, ct);
            return NoContent();
        }
        [HttpPost("stat")]
        public async Task<IActionResult> UpsertStat([FromBody] UpsertMonsterStatRequest request, CancellationToken ct)
        {
            await _service.UpsertStatAsync(request, ct);
            return NoContent();
        }
    }
}
