using Application.Combat;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("combat")]
    public sealed class CombatController : ControllerBase
    {
        private readonly ICombatService _service;

        public CombatController(ICombatService service) => _service = service;

        [HttpPost("simulate")]
        [ProducesResponseType(typeof(SimulateCombatResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> Simulate([FromBody] SimulateCombatRequest req, CancellationToken ct)
        {
            var res = await _service.SimulateAsync(req, ct);
            return Ok(res);
        }

        [HttpGet("{combatId:long}/log")]
        [ProducesResponseType(typeof(CombatLogPageDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLog([FromRoute] long combatId, [FromQuery] string? cursor, [FromQuery] int size = 200, CancellationToken ct = default)
        {
            var res = await _service.GetLogAsync(combatId, cursor, size, ct);
            return Ok(res);
        }
        [HttpGet("{combatId:long}/summary")]
        [ProducesResponseType(typeof(CombatLogSummaryDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSummary([FromRoute] long combatId, CancellationToken ct)
        {
            var res = await _service.GetSummaryAsync(combatId, ct);
            return Ok(res);
        }
    }
}
