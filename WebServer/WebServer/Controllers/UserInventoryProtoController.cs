
using Application.UserInventory;
using Contracts.Protos; 
using Microsoft.AspNetCore.Mvc;
using WebServer.Mappers;
using ConsumeItemRequest = Contracts.Protos.ConsumeItemRequest;




namespace WebServer.Controllers
{
    [ApiController]
    [Route("api/pb/users/{userId:int}/inventory")]
    [Produces("application/x-protobuf")]
    public class UserInventoryProtoController : ControllerBase
    {
        private readonly IUserInventoryService _svc;

        public UserInventoryProtoController(IUserInventoryService svc)
        {
            _svc = svc;
        }

        #region Get 
        // GET /api/pb/users/{userId}/inventory?page=&pageSize=&itemId=&updatedFrom=&updatedTo=
        [HttpGet]
        public async Task<ActionResult<ListUserInventoryResponse>> List(
            int userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] int? itemId = null,
            [FromQuery] System.DateTimeOffset? updatedFrom = null,
            [FromQuery] System.DateTimeOffset? updatedTo = null,
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

            var paged = await _svc.GetListAsync(query, ct);
            return Ok(paged.ToPb()); // PagedResult<UserInventoryDto> → ListUserInventoryResponse
        }
        // GET /api/pb/users/{userId}/inventory/{itemId}
        [HttpGet("{itemId:int}")]
        public async Task<ActionResult<GetUserInventoryResponse>> GetOne(
            int userId,
            int itemId,
            CancellationToken ct = default)
        {
            var dto = await _svc.GetOneAsync(userId, itemId, ct);
            if (dto is null) return NotFound();
            return Ok(dto.ToPbGet()); // UserInventoryDto → GetUserInventoryResponse
        }
        #endregion

        #region 
        // POST /api/pb/users/{userId}/inventory/grant
        [HttpPost("grant")]
        [Consumes("application/x-protobuf")]
        public async Task<ActionResult<GrantItemResponse>> Grant(
            int userId,
            [FromBody] Contracts.Protos.GrantItemRequest body,
            CancellationToken ct = default)
        {
            // 경로 우선 적용
            var req = new Application.UserInventory.GrantItemRequest(
                UserId: userId,
                ItemId: body.ItemId,
                Amount: body.Amount
            );

            var dto = await _svc.GrantAsync(req, ct);
            return Ok(dto.ToPbGrant()); // UserInventoryDto → GrantItemResponse
        }

        // POST /api/pb/users/{userId}/inventory/consume
        [HttpPost("consume")]
        [Consumes("application/x-protobuf")]
        public async Task<ActionResult<ConsumeItemResponse>> Consume(
            int userId,
            [FromBody]  ConsumeItemRequest body,
            CancellationToken ct = default)
        {
            var req = new Application.UserInventory.ConsumeItemRequest(
                UserId: userId,
                ItemId: body.ItemId,
                Amount: body.Amount
            );

            var result = await _svc.ConsumeAsync(req, ct);

            // 매핑(네 서비스의 ConsumeResultDto 구조에 맞춘 버전)
            var pb = result.ToPb();

            if (!result.Success)
                return BadRequest(pb);

            return Ok(pb);
        }

        // PUT /api/pb/users/{userId}/inventory/{itemId}
        [HttpPut("{itemId:int}")]
        [Consumes("application/x-protobuf")]
        public async Task<ActionResult<SetItemCountResponse>> SetCount(
            int userId,
            int itemId,
            [FromBody] Contracts.Protos.SetItemCountRequest body,
            CancellationToken ct = default)
        {
            var req = new Application.UserInventory.SetItemCountRequest(
                UserId: userId,
                ItemId: itemId,
                NewCount: body.NewCount
            );

            var dto = await _svc.SetCountAsync(req, ct);
            return Ok(dto.ToPbSet()); // UserInventoryDto → SetItemCountResponse
        }

        // DELETE /api/pb/users/{userId}/inventory/{itemId}
        [HttpDelete("{itemId:int}")]
        public async Task<IActionResult> Delete(
            int userId,
            int itemId,
            CancellationToken ct = default)
        {
            var req = new Application.UserInventory.DeleteItemRequest(UserId: userId, ItemId: itemId);
            await _svc.DeleteAsync(req, ct);
            return NoContent();
        }

        // (선택) GET /api/pb/inventory/owners?itemId=&page=&pageSize=&minCount=
        [HttpGet("~/api/pb/inventory/owners")]
        public async Task<ActionResult<ListOwnersResponse>> GetOwners(
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

            var paged = await _svc.GetOwnersAsync(query, ct);

            var resp = new ListOwnersResponse
            {
                PageInfo = new PageInfo
                {
                    Page = paged.Page,
                    PageSize = paged.PageSize,
                    TotalCount = (int)paged.TotalCount
                }
            };
            resp.Owners.AddRange(paged.Items.Select(i => i.ToPb()));

            return Ok(resp);
        }
        #endregion
    }
}
