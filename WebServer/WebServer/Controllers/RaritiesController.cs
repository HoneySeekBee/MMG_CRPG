using Application.Rarities;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // /api/rarities
    public sealed class RaritiesController : ControllerBase
    {
        private readonly IRarityService _svc;
        private readonly ILogger<RaritiesController> _logger;
        public RaritiesController(IRarityService svc, ILogger<RaritiesController> logger)
        { _svc = svc; _logger = logger; }

        // GET /api/rarities?isActive=true&stars=5&page=1&pageSize=50
        [HttpGet]
        public async Task<IActionResult> List([FromQuery] bool? isActive, [FromQuery] int? stars,
            [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken ct = default)
            => Ok(await _svc.ListAsync(isActive, stars, page, pageSize, ct));

        // GET /api/rarities/10
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id, CancellationToken ct = default)
            => (await _svc.GetAsync(id, ct)) is { } dto ? Ok(dto) : NotFound();

        // POST /api/rarities
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRarityRequest req, CancellationToken ct = default)
        {
            try
            {
                var created = await _svc.CreateAsync(req, ct);
                return CreatedAtAction(nameof(Get), new { id = created.RarityId }, created);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create Rarity failed");
                return Problem("Rarity 생성 실패", statusCode: 500);
            }
        }

        // PUT /api/rarities/10
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateRarityRequest req, CancellationToken ct = default)
        {
            try { await _svc.UpdateAsync(id, req, ct); return NoContent(); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update Rarity failed");
                return Problem("Rarity 수정 실패", statusCode: 500);
            }
        }

        // DELETE /api/rarities/10
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
        {
            try { await _svc.DeleteAsync(id, ct); return NoContent(); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete Rarity failed");
                return Problem("Rarity 삭제 실패", statusCode: 500);
            }
        }
    }
}
