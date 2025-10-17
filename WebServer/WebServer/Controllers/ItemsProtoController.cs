using Application.Items;
using Contracts.Protos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebServer.Options;
using WebServer.Mappers;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("api/pb/items")]
    [Produces("application/x-protobuf")]
    public sealed class ItemsProtoController : ControllerBase
    {
        private readonly IItemCache _cache;
        private readonly string _imageBase; // ex) https://cdn.example.com
        private readonly string _iconsSubdir;
        private readonly string _portraitsSubdir;

        public ItemsProtoController(IItemCache cache, IOptions<AssetsOptions> assetsOpt)
        {
            _cache = cache;
            var o = assetsOpt.Value;
            _imageBase = (o.ImageUrl ?? "").TrimEnd('/');
            _iconsSubdir = o.IconsSubdir ?? "icons";
            _portraitsSubdir = o.PortraitsSubdir ?? "portraits";
        }

        // URL 조립 (버전 쿼리 등 필요하면 여기에서)
        private string? IconUrl(int? iconId)
            => iconId is > 0 ? $"{_imageBase}/{_iconsSubdir}/{iconId}.png" : null;

        private string? PortraitUrl(int? portraitId)
            => portraitId is > 0 ? $"{_imageBase}/{_portraitsSubdir}/{portraitId}.png" : null;

        // GET /api/pb/items?search=&typeId=&rarityId=&activeOnly=&page=&pageSize=
        [HttpGet]
        public ActionResult<ListItemsResponse> List(
            [FromQuery] string? search,
            [FromQuery] int typeId = 0,
            [FromQuery] int rarityId = 0,
            [FromQuery] bool activeOnly = true,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 200) pageSize = 50;

            var q = _cache.GetAll().AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                q = q.Where(x => x.Name.Contains(s, StringComparison.OrdinalIgnoreCase)
                              || x.Code.Contains(s, StringComparison.OrdinalIgnoreCase));
            }
            if (typeId > 0) q = q.Where(x => x.TypeId == typeId);
            if (rarityId > 0) q = q.Where(x => x.RarityId == rarityId);
            if (activeOnly) q = q.Where(x => x.IsActive);

            var total = q.Count();

            var pageItems = q
                .OrderBy(x => x.Code) // 정렬 기준 필요한 대로
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => x.ToDetailPb(IconUrl, PortraitUrl))
                .ToList();

            return Ok(new ListItemsResponse
            {
                TotalCount = total,
                Items = { pageItems }
            });
        }

        // GET /api/pb/items/{id}
        [HttpGet("{id:long}")]
        public ActionResult<GetItemResponse> Get(long id)
        {
            var dto = _cache.GetById(id);
            if (dto is null) return NotFound();

            return Ok(new GetItemResponse
            {
                Item = dto.ToDetailPb(IconUrl, PortraitUrl)
            });
        }
    }
}
