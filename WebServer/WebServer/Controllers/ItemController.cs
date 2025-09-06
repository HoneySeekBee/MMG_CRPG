using Application.Items;
using Domain.Enum;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // /api/items
    public sealed class ItemsController : ControllerBase
    {
        private readonly IItemService _svc;
        private readonly ILogger<ItemsController> _logger;

        public ItemsController(IItemService svc, ILogger<ItemsController> logger)
        {
            _svc = svc;
            _logger = logger;
        }

        // GET /api/items?typeId=1&rarityId=2&isActive=true&search=sword&tags=event&tags=limited&page=1&pageSize=50
        [HttpGet]
        public async Task<IActionResult> List([FromQuery] ListItemsRequest req, CancellationToken ct = default)
        {
            var result = await _svc.ListAsync(req, ct);
            return Ok(result);
        }

        // GET /api/items/123
        [HttpGet("{id:long}")]
        public async Task<IActionResult> Get(long id, CancellationToken ct = default)
        {
            var dto = await _svc.GetAsync(id, ct);
            return dto is null ? NotFound() : Ok(dto);
        }

        // POST /api/items
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateItemRequest req, CancellationToken ct = default)
        {
            try
            {
                var created = await _svc.CreateAsync(req, ct);
                return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create Item failed");
                return Problem("Item 생성 실패", statusCode: 500);
            }
        }

        // PATCH /api/items  (부분 수정)
        [HttpPatch]
        public async Task<IActionResult> Patch([FromBody] UpdateItemRequest req, CancellationToken ct = default)
        {
            try
            {
                var updated = await _svc.UpdateAsync(req, ct);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update Item failed");
                return Problem("Item 수정 실패", statusCode: 500);
            }
        }

        // DELETE /api/items/123
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id, CancellationToken ct = default)
        {
            try
            {
                await _svc.DeleteAsync(id, ct);
                return NoContent();
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete Item failed");
                return Problem("Item 삭제 실패", statusCode: 500);
            }
        }

        // ---------------- 하위 엔티티: Stats ----------------

        // PUT /api/items/123/stats  (Upsert 1개)
        [HttpPut("{id:long}/stats")]
        public async Task<IActionResult> UpsertStat(long id, [FromBody] UpsertStatRequest req, CancellationToken ct = default)
        {
            try
            {
                var dto = await _svc.UpsertStatAsync(id, req, ct);
                return Ok(dto);
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Upsert ItemStat failed");
                return Problem("ItemStat 저장 실패", statusCode: 500);
            }
        }

        // DELETE /api/items/123/stats/10
        [HttpDelete("{id:long}/stats/{statId:int}")]
        public async Task<IActionResult> RemoveStat(long id, int statId, CancellationToken ct = default)
        {
            try
            {
                var dto = await _svc.RemoveStatAsync(id, statId, ct);
                return Ok(dto);
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Remove ItemStat failed");
                return Problem("ItemStat 삭제 실패", statusCode: 500);
            }
        }

        // ---------------- 하위 엔티티: Effects ----------------

        // POST /api/items/123/effects
        [HttpPost("{id:long}/effects")]
        public async Task<IActionResult> AddEffect(long id, [FromBody] AddEffectRequest req, CancellationToken ct = default)
        {
            try
            {
                var dto = await _svc.AddEffectAsync(id, req, ct);
                return Ok(dto);
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Add ItemEffect failed");
                return Problem("ItemEffect 추가 실패", statusCode: 500);
            }
        }

        [HttpPatch("{id:long}/effects/{effectId:long}")]
        public async Task<IActionResult> UpdateEffect(long id, long effectId, [FromBody] UpdateEffectRequest req, CancellationToken ct = default)
        {
            if (req is null) return BadRequest("Body required.");

            var fixedReq = new UpdateEffectRequest
            {
                ItemId = id,
                EffectId = effectId,
                Scope = req.Scope,
                Payload = req.Payload,
                SortOrder = req.SortOrder
            };

            try
            {
                var dto = await _svc.UpdateEffectAsync(fixedReq, ct);
                return Ok(dto);
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update ItemEffect failed");
                return Problem("ItemEffect 수정 실패", statusCode: 500);
            }
        }
        // DELETE /api/items/123/effects/456
        [HttpDelete("{id:long}/effects/{effectId:long}")]
        public async Task<IActionResult> RemoveEffect(long id, long effectId, CancellationToken ct = default)
        {
            try
            {
                var dto = await _svc.RemoveEffectAsync(id, effectId, ct);
                return Ok(dto);
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Remove ItemEffect failed");
                return Problem("ItemEffect 삭제 실패", statusCode: 500);
            }
        }

        // ---------------- 하위 엔티티: Prices ----------------

        // PUT /api/items/123/prices
        [HttpPut("{id:long}/prices")]
        public async Task<IActionResult> SetPrice(long id, [FromBody] SetPriceRequest req, CancellationToken ct = default)
        {
            try
            {
                var dto = await _svc.SetPriceAsync(id, req, ct);
                return Ok(dto);
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Set ItemPrice failed");
                return Problem("ItemPrice 저장 실패", statusCode: 500);
            }
        }

        // DELETE /api/items/123/prices/1?priceType=Buy
        [HttpDelete("{id:long}/prices/{currencyId:int}")]
        public async Task<IActionResult> RemovePrice(long id, int currencyId, [FromQuery] ItemPriceType priceType, CancellationToken ct = default)
        {
            try
            {
                var dto = await _svc.RemovePriceAsync(id, currencyId, priceType, ct);
                return Ok(dto);
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Remove ItemPrice failed");
                return Problem("ItemPrice 삭제 실패", statusCode: 500);
            }
        }
    }
}
