using Application.Synergy;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces(MediaTypeNames.Application.Json)]
    public sealed class SynergiesController : ControllerBase
    {
        private readonly ISynergyService _service;
        public SynergiesController(ISynergyService service) => _service = service;

        /// <summary>시너지 단건 조회 (id)</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(SynergyDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct)
        {
            var s = await _service.GetAsync(id, ct);
            return s is null ? NotFound() : Ok(s);
        }

        /// <summary>시너지 단건 조회 (key)</summary>
        [HttpGet("by-key/{key}")]
        [ProducesResponseType(typeof(SynergyDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByKey([FromRoute] string key, CancellationToken ct)
        {
            var s = await _service.GetByKeyAsync(key, ct);
            return s is null ? NotFound() : Ok(s);
        }

        /// <summary>현재 활성 시너지 조회</summary>
        [HttpGet("actives")]
        [ProducesResponseType(typeof(IReadOnlyList<SynergyDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActives([FromQuery] DateTime? now, CancellationToken ct)
        {
            var at = now ?? DateTime.UtcNow;
            var list = await _service.GetActivesAsync(at, ct);
            return Ok(list);
        }
        private static DateTime? EnsureUtc(DateTime? dt)
        {
            if (dt is null) return null;
            var v = dt.Value;
            return v.Kind switch
            {
                DateTimeKind.Utc => v,
                DateTimeKind.Local => v.ToUniversalTime(),
                _ => DateTime.SpecifyKind(v, DateTimeKind.Utc) // Unspecified이면 UTC로 지정
            };
        }
        /// <summary>시너지 생성</summary>
        [HttpPost]
        [ProducesResponseType(typeof(SynergyDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateSynergyRequest req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.Key)) return BadRequest("Key is required.");
            if (req.Effect is null) return BadRequest("Effect is required.");
            req = req with
            {
                StartAt = EnsureUtc(req.StartAt),
                EndAt = EnsureUtc(req.EndAt)
            };
            var created = await _service.CreateAsync(req, ct);
            // Location: GET by id
            return CreatedAtAction(nameof(GetById), new { id = created.SynergyId }, created);
        }

        /// <summary>시너지 수정(부분 갱신)</summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(SynergyDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateSynergyRequest req, CancellationToken ct)
        {
            if (req.SynergyId != 0 && req.SynergyId != id)
                return BadRequest("Route id and body id mismatch.");
            req = req with
            {
                StartAt = EnsureUtc(req.StartAt),
                EndAt = EnsureUtc(req.EndAt)
            };
            try
            {
                var updated = await _service.UpdateAsync(req, ct);
                return Ok(updated);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        /// <summary>시너지 삭제</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
        {
            await _service.DeleteAsync(id, ct);
            return NoContent();
        }

        [HttpPost("evaluate")]
        [ProducesResponseType(typeof(IReadOnlyList<EvaluateResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Evaluate([FromBody] EvaluateSynergiesRequest req, CancellationToken ct)
        {
            if (req is null)
                return BadRequest("Request body is required.");

            // null 이 들어와도 빈 컬렉션으로 정규화(400 줄이기)
            var elementIds = req.ElementIds ?? Array.Empty<int>();
            var factionIds = req.FactionIds ?? Array.Empty<int>();
            var characters = req.Characters ?? Array.Empty<CharacterEquipSummary>();

            // 기본 형식 검증

            if (elementIds.Any(id => id <= 0) || factionIds.Any(id => id <= 0))
                return BadRequest("ElementIds/FactionIds must contain positive integers.");

            foreach (var ch in characters)
            {
                if (ch is null)
                    return BadRequest("Characters must not contain null items.");

                var tags = ch.TagCounts; // IReadOnlyDictionary<string,int>?
                if (tags is null) continue; // null 허용 시 건너뜀(또는 빈 사전으로 간주)

                if (tags.Any(kv => string.IsNullOrWhiteSpace(kv.Key)))
                    return BadRequest("TagCounts must use non-empty tag codes.");
                if (tags.Any(kv => kv.Value < 0))
                    return BadRequest("TagCounts must use non-negative counts.");
            }

            // 서비스는 req 자체를 넘겨도 되고, 필요한 경우 래핑 DTO 만들어 넘겨도 OK
            var result = await _service.EvaluateAsync(req, ct);
            return Ok(result);
        }
    }
}
