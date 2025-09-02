using Application.Factions;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // /api/factions
    public sealed class FactionsController : ControllerBase
    {
        private readonly IFactionService _svc;
        private readonly ILogger<FactionsController> _logger;
        public FactionsController(IFactionService svc, ILogger<FactionsController> logger)
        {
            _svc = svc; _logger = logger;
        }

        // GET /api/factions?isActive=true&page=1&pageSize=50
        [HttpGet]
        public async Task<IActionResult> List([FromQuery] bool? isActive, [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken ct = default)
            => Ok(await _svc.ListAsync(isActive, page, pageSize, ct));

        // GET /api/factions/123
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id, CancellationToken ct = default)
            => (await _svc.GetAsync(id, ct)) is { } dto ? Ok(dto) : NotFound();

        // POST /api/factions
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFactionRequest req, CancellationToken ct = default)
        {
            try
            {
                var created = await _svc.CreateAsync(req, ct);
                return CreatedAtAction(nameof(Get), new { id = created.FactionId }, created);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create Faction failed");
                return Problem("Faction 생성 실패", statusCode: 500);
            }
        }

        // PUT /api/factions/123
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateFactionRequest req, CancellationToken ct = default)
        {
            try { await _svc.UpdateAsync(id, req, ct); return NoContent(); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update Faction failed");
                return Problem("Faction 수정 실패", statusCode: 500);
            }
        }

        // DELETE /api/factions/123
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
        {
            try { await _svc.DeleteAsync(id, ct); return NoContent(); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete Faction failed");
                return Problem("Faction 삭제 실패", statusCode: 500);
            }
        }
    }
}
