using Application.Elements;
using Application.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ElementController : ControllerBase
    {
        private readonly IElementService _svc;
        private readonly IElementRepository _repo;

        public ElementController(IElementService svc, IElementRepository repo)
        {
            _svc = svc; _repo = repo;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ElementDto>> GetById(int id, CancellationToken ct)
            => Ok(await _svc.GetByIdAsync(id, ct));

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ElementDto>>> List([FromQuery] bool? isActive, [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken ct = default)
            => Ok(await _svc.ListAsync(isActive, search, page, pageSize, ct));

        [HttpPost]
        public async Task<ActionResult<int>> Create([FromBody] CreateElementRequest req, CancellationToken ct)
        {
            var id = await _svc.CreateAsync(req, ct);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateElementRequest req, CancellationToken ct)
        {
            await _svc.UpdateAsync(id, req, ct);
            await _repo.SaveChangesAsync(ct); // 심플: 여기서 저장
            return NoContent();
        }

        [HttpPatch("{id:int}/active")]
        public async Task<IActionResult> SetActive(int id, [FromQuery] bool value, CancellationToken ct)
        {
            await _svc.SetActiveAsync(id, value, ct);
            await _repo.SaveChangesAsync(ct); // 심플: 여기서 저장
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var e = await _svc.GetByIdAsync(id, ct); // 존재 확인
            var entity = await (_repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException()); // 실제 엔티티 획득
            await _repo.RemoveAsync(entity, ct);
            return NoContent();
        }
    }
}
