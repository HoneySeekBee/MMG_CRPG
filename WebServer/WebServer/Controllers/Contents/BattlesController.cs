using Application.Contents.Battles;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Controllers.Contents
{
    [ApiController]
    [Route("api/[controller]")]
    public class BattlesController : ControllerBase
    {
        private readonly IBattlesService _battleService;

        public BattlesController(IBattlesService battleService)
        {
            _battleService = battleService;
        }

        // GET: api/battles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BattleDto>>> GetList(CancellationToken cancellationToken)
        {
            var battles = await _battleService.GetListAsync(cancellationToken);
            return Ok(battles);
        }

        // GET: api/battles/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<BattleDto>> GetById(int id, CancellationToken cancellationToken)
        {
            var battle = await _battleService.GetByIdAsync(id, cancellationToken);
            if (battle is null)
                return NotFound();

            return Ok(battle);
        }

        // POST: api/battles
        [HttpPost]
        public async Task<ActionResult<int>> Create([FromBody] CreateBattleRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var id = await _battleService.CreateAsync(request, cancellationToken);
            // Location 헤더 달고 싶으면 CreatedAtAction 써도 됨
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        // PUT: api/battles/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBattleRequest request, CancellationToken cancellationToken)
        {
            if (id != request.Id)
                return BadRequest("id mismatch");

            var ok = await _battleService.UpdateAsync(request, cancellationToken);
            if (!ok)
                return NotFound();

            return NoContent();
        }

        // DELETE: api/battles/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var ok = await _battleService.DeleteAsync(id, cancellationToken);
            if (!ok)
                return NotFound();

            return NoContent();
        }
    }
}
