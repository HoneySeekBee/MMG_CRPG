using Application.Items;
using Application.ItemTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]          // /api/itemtypes
    public sealed class ItemTypesController : ControllerBase
    {
        private readonly IItemTypeService _svc;
        private readonly ILogger<ItemTypesController> _log;

        public ItemTypesController(IItemTypeService svc, ILogger<ItemTypesController> log)
        { _svc = svc; _log = log; }

        // GET /api/itemtypes?search=&hasSlot=&sort=&desc=&page=&pageSize=
        [HttpGet]
        public async Task<IActionResult> List([FromQuery] ListItemTypesRequest req, CancellationToken ct)
            => Ok(await _svc.ListAsync(req, ct));

        // GET /api/itemtypes/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(short id, CancellationToken ct)
            => (await _svc.GetAsync(id, ct)) is { } dto ? Ok(dto) : NotFound();

        // POST /api/itemtypes
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateItemTypeRequest req, CancellationToken ct = default)
        {
            try
            {
                var created = await _svc.CreateAsync(req, ct);
                return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
            catch (DbUpdateException ex)
            {
                var msg = ex.InnerException?.Message ?? ex.Message;
                if (ex.InnerException is PostgresException pg)
                    msg = $"PG {pg.SqlState} {pg.ConstraintName}: {pg.MessageText}";
                return BadRequest(msg);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Create ItemType failed");
                return Problem(title: "ItemType 생성 실패", detail: ex.Message, statusCode: 500);
            }
        }

        // PUT /api/itemtypes/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(short id, [FromBody] UpdateItemTypeRequest req, CancellationToken ct)
        {
            await _svc.UpdateAsync(id, req with { Id = id }, ct);
            return NoContent();
        }

        // PATCH /api/itemtypes/5/slot
        [HttpPatch("{id:int}/slot")]
        public async Task<IActionResult> PatchSlot(short id, [FromBody] PatchItemTypeSlotRequest req, CancellationToken ct)
        {
            await _svc.PatchSlotAsync(id, req with { Id = id }, ct);
            return NoContent();
        }

        // DELETE /api/itemtypes/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(short id, CancellationToken ct)
        {
            await _svc.DeleteAsync(id, ct);
            return NoContent();
        }
    }
}
