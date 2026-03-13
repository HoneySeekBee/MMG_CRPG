using Application.Combat;
using Microsoft.AspNetCore.Mvc;

namespace CombatServer.Controllers
{
    [ApiController]
    [Route("combat")]
    public sealed class CombatTickController : ControllerBase
    {
        private readonly ICombatService _combat;

        public CombatTickController(ICombatService combat)
        {
            _combat = combat;
        }

        // POST /combat/init  (called by WebServer)
        [HttpPost("init")]
        public async Task<IActionResult> Init([FromBody] InitCombatPayload payload, CancellationToken ct)
        {
            try
            {
                var snapshot = await _combat.InitCombatAsync(payload, ct);
                return Ok(snapshot);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // GET /combat/{combatId}/result  (called by WebServer on finish)
        [HttpGet("{combatId:long}/result")]
        public async Task<IActionResult> GetResult(long combatId, CancellationToken ct)
        {
            try
            {
                var result = await _combat.GetResultAsync(combatId, ct);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // POST /combat/{combatId}/tick
        [HttpPost("{combatId:long}/tick")]
        public async Task<IActionResult> Tick(long combatId, [FromBody] TickRequest req, CancellationToken ct)
        {
            try
            {
                var result = await _combat.TickAsync(combatId, req.Tick, ct);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        // POST /combat/{combatId}/command
        [HttpPost("{combatId:long}/command")]
        public async Task<IActionResult> Command(long combatId, [FromBody] CombatCommandDto cmd, CancellationToken ct)
        {
            try
            {
                await _combat.EnqueueCommandAsync(combatId, cmd, ct);
                return Ok();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        // POST /combat/{combatId}/speed/toggle
        [HttpPost("{combatId:long}/speed/toggle")]
        public async Task<IActionResult> ToggleSpeed(long combatId, CancellationToken ct)
        {
            try
            {
                var speed = await _combat.ToggleSpeedAsync(combatId, ct);
                return Ok(new { speed = speed.ToString() });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        // GET /combat/{combatId}/log
        [HttpGet("{combatId:long}/log")]
        public async Task<IActionResult> GetLog(
            long combatId,
            [FromQuery] string? cursor,
            [FromQuery] int size = 100,
            CancellationToken ct = default)
        {
            try
            {
                var result = await _combat.GetLogAsync(combatId, cursor, size, ct);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        // GET /combat/{combatId}/summary
        [HttpGet("{combatId:long}/summary")]
        public async Task<IActionResult> GetSummary(long combatId, CancellationToken ct)
        {
            try
            {
                var result = await _combat.GetSummaryAsync(combatId, ct);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }
    }

    public sealed record TickRequest(int Tick);
}
