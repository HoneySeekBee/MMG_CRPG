using Application.Common.Models;
using Application.UserInventory;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Controllers.User
{
    [ApiController]
    [Route("api/users/{userId:int}/inventory")]
    public sealed class UserInventoryController : ControllerBase
    {
        private readonly IUserInventoryService _svc;

        public UserInventoryController(IUserInventoryService svc)
        {
            _svc = svc;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<UserInventoryDto>>> GetList(
           int userId,
           [FromQuery] int page = 1,
           [FromQuery] int pageSize = 50,
           [FromQuery] int? itemId = null,
           [FromQuery] DateTimeOffset? updatedFrom = null,
           [FromQuery] DateTimeOffset? updatedTo = null,
           CancellationToken ct = default)
        {
            var query = new UserInventoryListQuery(
                UserId: userId,
                Page: page,
                PageSize: pageSize,
                ItemId: itemId,
                UpdatedFrom: updatedFrom,
                UpdatedTo: updatedTo
            );

            var result = await _svc.GetListAsync(query, ct);
            return Ok(result);
        }

        [HttpGet("{itemId:int}")]
        public async Task<ActionResult<UserInventoryDto>> GetOne(
           int userId,
           int itemId,
           CancellationToken ct = default)
        {
            var dto = await _svc.GetOneAsync(userId, itemId, ct);
            if (dto is null) return NotFound();
            return Ok(dto);
        }
        [HttpPost("grant")]
        public async Task<ActionResult<UserInventoryDto>> Grant(
           int userId,
           [FromBody] GrantItemRequest body,
           CancellationToken ct = default)
        {
            // body.UserId를 경로와 일치시킴(경로 우선)
            var req = new GrantItemRequest(UserId: userId, ItemId: body.ItemId, Amount: body.Amount);
            var dto = await _svc.GrantAsync(req, ct);
            return Ok(dto);
        }
        [HttpPost("consume")]
        public async Task<ActionResult<IUserInventoryService.ConsumeResultDto>> Consume(
            int userId,
            [FromBody] ConsumeItemRequest body,
            CancellationToken ct = default)
        {
            var req = new ConsumeItemRequest(UserId: userId, ItemId: body.ItemId, Amount: body.Amount);
            var result = await _svc.ConsumeAsync(req, ct);
            if (!result.Success) return BadRequest(result); // 정책에 맞게 400/409 등 선택
            return Ok(result);
        }
        [HttpPut("{itemId:int}")]
        public async Task<ActionResult<UserInventoryDto>> SetCount(
            int userId,
            int itemId,
            [FromBody] SetItemCountRequest body,
            CancellationToken ct = default)
        {
            var req = new SetItemCountRequest(UserId: userId, ItemId: itemId, NewCount: body.NewCount);
            var dto = await _svc.SetCountAsync(req, ct);
            return Ok(dto);
        }

        [HttpDelete("{itemId:int}")]
        public async Task<IActionResult> Delete(
            int userId,
            int itemId,
            CancellationToken ct = default)
        {
            var req = new DeleteItemRequest(UserId: userId, ItemId: itemId);
            await _svc.DeleteAsync(req, ct);
            return NoContent();
        }
        [HttpGet("~/api/inventory/owners")]
        public async Task<ActionResult<PagedResult<UserInventoryDto>>> GetOwners(
           [FromQuery] int itemId,
           [FromQuery] int page = 1,
           [FromQuery] int pageSize = 50,
           [FromQuery] int? minCount = null,
           CancellationToken ct = default)
        {
            var query = new ItemOwnershipQuery(
                ItemId: itemId,
                Page: page,
                PageSize: pageSize,
                MinCount: minCount
            );

            var result = await _svc.GetOwnersAsync(query, ct);
            return Ok(result);
        }

    }
}
