using Application.ElementAffinities;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ElementAffinityController : ControllerBase
    {
        private readonly IElementAffinityService _svc;
        private readonly ILogger<ElementAffinityController> _logger;
        public ElementAffinityController(
            IElementAffinityService svc,
            ILogger<ElementAffinityController> logger)
        {
            _svc = svc;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ElementAffinityDto>>> List([FromQuery] int? attacker,
            [FromQuery] int? defender,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            CancellationToken ct = default)
        {
            try
            {
                var items = await _svc.ListAsync(attacker, defender, page, pageSize, ct);
                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ElementAffinity List 실패 (attacker:{attacker}, defender:{defender}, page:{page}, size:{pageSize})",
                    attacker, defender, page, pageSize);
                return Problem(title: "상성 목록 조회 실패", detail: ex.Message, statusCode: 500);
            }
        }

        [HttpGet("{attacker:int}/{defender:int}")]
        public async Task<ActionResult<ElementAffinityDto>> Get(
           int attacker, int defender, CancellationToken ct)
        {
            var dto = await _svc.GetAsync(attacker, defender, ct);
            return dto is null ? NotFound() : Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateElementAffinityRequest req,
            CancellationToken ct)
        {
            try
            {
                await _svc.CreateAsync(req, ct);
                return CreatedAtAction(nameof(Get),
                    new { attacker = req.AttackerElementId, defender = req.DefenderElementId },
                    value: null);
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(knf.Message);
            }
            catch (InvalidOperationException ioe)
            {
                return BadRequest(ioe.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ElementAffinity Create 실패");
                return Problem(title: "상성 생성 실패", detail: ex.Message, statusCode: 500);
            }
        }

        [HttpPut("{attacker:int}/{defender:int}")]
        public async Task<IActionResult> Update(
            int attacker, int defender,
            [FromBody] UpdateElementAffinityRequest req,
            CancellationToken ct)
        {
            try
            {
                await _svc.UpdateAsync(attacker, defender, req, ct);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ioe)
            {
                return BadRequest(ioe.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ElementAffinity Update 실패 ({attacker},{defender})", attacker, defender);
                return Problem(title: "상성 업데이트 실패", detail: ex.Message, statusCode: 500);
            }
        }

        [HttpDelete("{attacker:int}/{defender:int}")]
        public async Task<IActionResult> Delete(
            int attacker, int defender, CancellationToken ct)
        {
            try
            {
                await _svc.DeleteAsync(attacker, defender, ct);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ElementAffinity Delete 실패 ({attacker},{defender})", attacker, defender);
                return Problem(title: "상성 삭제 실패", detail: ex.Message, statusCode: 500);
            }
        }
    }
}
