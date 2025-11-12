using Application.Portraits;
using Contracts.Assets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebServer.Options;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("api/pb/portraits")]
    [Produces("application/x-protobuf")]
    public sealed class PortraitsProtoController : ControllerBase
    {
        private readonly IPortraitsCache _cache;
        private readonly string _base;
        private readonly string _dir;

        public PortraitsProtoController(IPortraitsCache cache, IOptions<AssetsOptions> opt)
        {
            _cache = cache;
            var o = opt.Value;
            _base = (o.ImageUrl ?? "").TrimEnd('/');
            _dir = o.PortraitsSubdir ?? "portraits";
        }

        private string BuildUrl(string key, int version)
            => $"{_base}/{_dir}/{key}.png?v={version}";

        // GET /api/pb/portraits
        [HttpGet]
        public ActionResult<ListPortraitsResponse> List()
        {
            var data = _cache.GetAll()
                .Select(m => new PortraitMessage
                {
                    PortraitId = m.PortraitId,
                    Key = m.Key,
                    Version = m.Version,
                    Url = BuildUrl(m.Key, m.Version)
                })
                .ToList();

            return Ok(new ListPortraitsResponse
            {
                TotalCount = data.Count,
                Portraits = { data }
            });
        }

        // GET /api/pb/portraits/{id}
        [HttpGet("{id:int}")]
        public ActionResult<GetPortraitResponse> Get(int id)
        {
            var m = _cache.GetAll().FirstOrDefault(x => x.PortraitId == id);
            if (m is null) return NotFound();

            return Ok(new GetPortraitResponse
            {
                Portrait = new PortraitMessage
                {
                    PortraitId = m.PortraitId,
                    Key = m.Key,
                    Version = m.Version,
                    Url = BuildUrl(m.Key, m.Version)
                }
            });
        }
    }
}
