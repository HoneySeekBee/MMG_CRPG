using Application.ItemTypes;
using Contracts.Protos;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("api/pb/itemtypes")]
    [Produces("application/x-protobuf")]
    public sealed class ItemTypesProtoController : ControllerBase
    {
        private readonly IItemTypeCache _cache;
        public ItemTypesProtoController(IItemTypeCache cache) => _cache = cache;

        [HttpGet]
        public ActionResult<ListItemTypesResponseMessage> List([FromQuery] bool activeOnly)
        {
            var items = (activeOnly ? _cache.GetAll().Where(x => x.Active) : _cache.GetAll())
                .Select(Map)
                .ToList();
            Console.WriteLine($"ItemType 요청 {items.Count}");
            return Ok(new ListItemTypesResponseMessage
            {
                Items = { items },
                TotalCount = items.Count
            });
        }

        [HttpGet("{id:int}")]
        public IActionResult Get(short id)
        {
            var dto = _cache.GetById(id);
            return dto is null ? NotFound() : Ok(Map(dto));
        }

        private static ItemTypeMessage Map(ItemTypeDto x) => new()
        {
            Id = x.Id,
            Code = x.Code,
            Name = x.Name,
            SlotId = x.SlotId ?? 0,
            Active = x.Active,
            CreatedAt = x.CreatedAt.ToUnixTimeMilliseconds(),
            UpdatedAt = x.UpdatedAt.ToUnixTimeMilliseconds()
        };
    }
}
