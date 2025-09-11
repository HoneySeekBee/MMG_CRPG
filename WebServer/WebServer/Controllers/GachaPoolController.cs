using Application.GachaPool;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("api/gacha/pools")]
    public sealed class GachaPoolController : ControllerBase
    {
        private readonly IGachaPoolService _svc;
        public GachaPoolController(IGachaPoolService svc) => _svc = svc;

        // ─────────────────────────────────────────────────────────────
        // 검색/목록
        // GET /api/gacha/pools?keyword=&skip=0&take=20
        // ─────────────────────────────────────────────────────────────
        [HttpGet]
        [ProducesResponseType(typeof(SearchResponse<GachaPoolDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Search([FromQuery] string? keyword, [FromQuery] int skip = 0, [FromQuery] int take = 20, CancellationToken ct = default)
        {
            var (items, total) = await _svc.SearchAsync(new QueryGachaPoolsRequest(keyword, skip, take), ct);
            return Ok(new SearchResponse<GachaPoolDto>(items, total, skip, take));
        }

        // 드롭다운용 간단 목록
        // GET /api/gacha/pools/list?take=100
        [HttpGet("list")]
        [ProducesResponseType(typeof(IReadOnlyList<GachaPoolDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> List([FromQuery] int take = 100, CancellationToken ct = default)
        {
            var items = await _svc.ListAsync(take, ct);
            return Ok(items);
        }

        // ─────────────────────────────────────────────────────────────
        // 단건 조회
        // GET /api/gacha/pools/{id}
        // ─────────────────────────────────────────────────────────────
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(GachaPoolDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(int id, CancellationToken ct = default)
        {
            var dto = await _svc.GetDetailAsync(id, ct);
            return dto is null ? NotFound() : Ok(dto);
        }

        // ─────────────────────────────────────────────────────────────
        // 생성
        // POST /api/gacha/pools
        // ─────────────────────────────────────────────────────────────
        [HttpPost]
        [ProducesResponseType(typeof(IdOnly), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateGachaPoolRequest req, CancellationToken ct = default)
        {
            if (req is null) return ValidationProblem("Request body is required.");
            try
            {
                var dto = await _svc.CreateAsync(req, ct);
                return CreatedAtAction(nameof(Get), new { id = dto.PoolId }, new IdOnly(dto.PoolId));
            }
            catch (ArgumentException ex)
            {
                return ValidationProblem(ex.Message);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // 수정
        // PUT /api/gacha/pools/{id}
        // ─────────────────────────────────────────────────────────────
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(GachaPoolDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateGachaPoolRequest req, CancellationToken ct = default)
        {
            if (req is null || req.PoolId != id) return ValidationProblem("Id mismatch.");
            try
            {
                var dto = await _svc.UpdateAsync(req, ct);
                return Ok(dto);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                return ValidationProblem(ex.Message);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // 삭제
        // DELETE /api/gacha/pools/{id}
        // ─────────────────────────────────────────────────────────────
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
        {
            await _svc.DeleteAsync(id, ct);
            return NoContent();
        }

        // ─────────────────────────────────────────────────────────────
        // 엔트리 벌크 업서트(확률표 교체)
        // PUT /api/gacha/pools/{id}/entries
        // body: { "poolId": (선택) , "entries": [ { characterId, grade, rateUp, weight }, ... ] }
        // ─────────────────────────────────────────────────────────────
        [HttpPut("{id:int}/entries")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ReplaceEntries(int id, [FromBody] UpsertGachaPoolEntriesRequest body, CancellationToken ct = default)
        {
            if (body is null) return ValidationProblem("Request body is required.");
            var req = body with { PoolId = id };

            try
            {
                await _svc.ReplaceEntriesAsync(req, ct);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                return ValidationProblem(ex.Message);
            }
        }

        // 단순 응답 컨테이너들
        public sealed record IdOnly(int Id);
        public sealed record SearchResponse<T>(IReadOnlyList<T> Items, int Total, int Skip, int Take);
    }
}
