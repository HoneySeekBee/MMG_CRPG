using Application.Icons;
using Contracts.Assets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebServer.Options;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("api/pb/icons")]
    [Produces("application/x-protobuf")]
    public sealed class IconsProtoController : ControllerBase
    {
        private readonly IIconCache _cache;
        private readonly string _base;
        private readonly string _dir;

        public IconsProtoController(IIconCache cache, IOptions<AssetsOptions> opt)
        {
            _cache = cache;
            var o = opt.Value;
            _base = (o.ImageUrl ?? "").TrimEnd('/');
            _dir = o.IconsSubdir ?? "icons";
        }

        private string BuildUrl(string key, int version)
            => $"{_base}/{_dir}/{key}.png?v={version}";

        // GET /api/pb/icons
        [HttpGet]
        public ActionResult<ListIconsResponse> List()
        {
            var data = _cache.GetAll()
                .Select(m => new IconMessage
                {
                    IconId = m.IconId,
                    Key = m.Key,
                    Version = m.Version,
                    Url = BuildUrl(m.Key, m.Version)
                })
                .ToList();

            return Ok(new ListIconsResponse
            {
                TotalCount = data.Count,
                Icons = { data }
            });
        }

        // GET /api/pb/icons/{id}
        [HttpGet("{id:int}")]
        public ActionResult<GetIconResponse> Get(int id)
        {
            var m = _cache.GetAll().FirstOrDefault(x => x.IconId == id);
            if (m is null) return NotFound();

            return Ok(new GetIconResponse
            {
                Icon = new IconMessage
                {
                    IconId = m.IconId,
                    Key = m.Key,
                    Version = m.Version,
                    Url = BuildUrl(m.Key, m.Version)
                }
            });
        }
    }
}
