using Application.GachaBanner;
using Domain.Enum;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class GachaBannerController : ControllerBase
    {
        private readonly IGachaBannerService _service;

        public GachaBannerController(IGachaBannerService service)
            => _service = service;

        // GET: api/GachaBanner/live?take=10
        [HttpGet("live")]
        [ProducesResponseType(typeof(IReadOnlyList<GachaBannerDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLiveAsync([FromQuery] int take = 10, CancellationToken ct = default)
        {
            var items = await _service.ListLiveAsync(take, ct);
            return Ok(items);
        }

        // GET: api/GachaBanner/5
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(GachaBannerDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAsync([FromRoute] int id, CancellationToken ct = default)
        {
            var dto = await _service.GetAsync(id, ct);
            return dto is null ? NotFound() : Ok(dto);
        }

        // GET: api/GachaBanner/by-key/bn_fes_001
        [HttpGet("by-key/{key}")]
        [ProducesResponseType(typeof(GachaBannerDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByKeyAsync([FromRoute] string key, CancellationToken ct = default)
        {
            var dto = await _service.GetByKeyAsync(key, ct);
            return dto is null ? NotFound() : Ok(dto);
        }

        // GET: api/GachaBanner?keyword=페스&skip=0&take=20
        [HttpGet]
        [ProducesResponseType(typeof(SearchResponse<GachaBannerDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchAsync([FromQuery] string? keyword = null, [FromQuery] int skip = 0, [FromQuery] int take = 20, CancellationToken ct = default)
        {
            var (items, total) = await _service.SearchAsync(new QueryGachaBannersRequest(keyword, skip, take), ct);
            return Ok(new SearchResponse<GachaBannerDto>(items, total, skip, take));
        }

        // POST: api/GachaBanner
        [HttpPost]
        [ProducesResponseType(typeof(GachaBannerDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAsync([FromBody] CreateGachaBannerRequest req, CancellationToken ct = default)
        {
            if (req is null) return BadRequest();

            var dto = await _service.CreateAsync(req, ct);
            return CreatedAtAction(nameof(GetAsync), new { id = dto.Id }, dto);
        }

        // PUT: api/GachaBanner/5
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(GachaBannerDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAsync([FromRoute] int id, [FromBody] UpdateGachaBannerRequest req, CancellationToken ct = default)
        {
            if (req is null || id != req.Id) return BadRequest("Id mismatch");

            try
            {
                var dto = await _service.UpdateAsync(req, ct);
                return Ok(dto);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // DELETE: api/GachaBanner/5
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteAsync([FromRoute] int id, CancellationToken ct = default)
        {
            await _service.DeleteAsync(id, ct);
            return NoContent();
        }

        // --- Partial updates (관리자용 간단 PATCH) ---

        // PATCH: api/GachaBanner/5/status
        [HttpPatch("{id:int}/status")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SetStatusAsync([FromRoute] int id, [FromBody] SetStatusBody body, CancellationToken ct = default)
        {
            try
            {
                await _service.SetStatusAsync(id, body.Status, ct);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // PATCH: api/GachaBanner/5/active
        [HttpPatch("{id:int}/active")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SetActiveAsync([FromRoute] int id, [FromBody] SetActiveBody body, CancellationToken ct = default)
        {
            try
            {
                await _service.SetActiveAsync(id, body.IsActive, ct);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // PATCH: api/GachaBanner/5/schedule
        [HttpPatch("{id:int}/schedule")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RescheduleAsync([FromRoute] int id, [FromBody] RescheduleBody body, CancellationToken ct = default)
        {
            try
            {
                await _service.RescheduleAsync(id, body.StartsAt, body.EndsAt, ct);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // PATCH: api/GachaBanner/5/priority
        [HttpPatch("{id:int}/priority")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SetPriorityAsync([FromRoute] int id, [FromBody] SetPriorityBody body, CancellationToken ct = default)
        {
            try
            {
                await _service.SetPriorityAsync(id, body.Priority, ct);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // --- 내부 바디 모델들 (간단 record) ---
        public sealed record SearchResponse<T>(IReadOnlyList<T> Items, int Total, int Skip, int Take);
        public sealed record SetStatusBody(GachaBannerStatus Status);
        public sealed record SetActiveBody(bool IsActive);
        public sealed record RescheduleBody(DateTimeOffset StartsAt, DateTimeOffset? EndsAt);
        public sealed record SetPriorityBody(short Priority);
    }
}
