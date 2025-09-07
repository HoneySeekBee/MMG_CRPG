using Application.StatTypes;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // /api/stattypes
    public sealed class StatTypesController : ControllerBase
    {
        private readonly IStatTypeService _svc;
        public StatTypesController(IStatTypeService svc) => _svc = svc;

        [HttpGet]
        public async Task<IEnumerable<StatTypeDto>> List(CancellationToken ct)
            => await _svc.ListAsync(ct);

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(short id, CancellationToken ct)
            => (await _svc.GetAsync(id, ct)) is { } x ? Ok(x) : NotFound();

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateStatTypeRequest req, CancellationToken ct)
        {
            var created = await _svc.CreateAsync(req, ct);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(short id, [FromBody] UpdateStatTypeRequest req, CancellationToken ct)
        {
            var updated = await _svc.UpdateAsync(id, req, ct);
            return Ok(updated);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(short id, CancellationToken ct)
        {
            await _svc.DeleteAsync(id, ct);
            return NoContent();
        }
    }
}
