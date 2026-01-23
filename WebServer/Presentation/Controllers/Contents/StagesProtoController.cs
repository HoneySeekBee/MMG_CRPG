using Application.Contents.Stages;
using Google.Protobuf;
using Microsoft.AspNetCore.Mvc;
using WebServer.Mappers.Contents;

namespace WebServer.Controllers.Contents
{
    [ApiController]
    [Route("api/pb/stages")]
    [Produces("application/x-protobuf")]
    public class StagesProtoController : ControllerBase
    {
        private readonly IStagesCache _cache;

        public StagesProtoController(IStagesCache cache)
        {
            _cache = cache;
        }

        // GET: api/proto/stages
        [HttpGet]
        public IActionResult GetAll()
        {
            var all = _cache.GetAll();
            var proto = StageProtoMapper.ToProto(all);
            var bytes = proto.ToByteArray();
            return File(bytes, "application/x-protobuf");
        }

        // GET: api/proto/stages/5
        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            var dto = _cache.GetById(id);
            if (dto is null)
                return NotFound();

            var proto = StageProtoMapper.ToProto(dto);
            var bytes = proto.ToByteArray();
            return File(bytes, "application/x-protobuf");
        }

        // 캐시 리로드 (컨텐츠 바꾼 뒤 호출)
        [HttpPost("reload")]
        public async Task<IActionResult> Reload(CancellationToken ct)
        {
            await _cache.ReloadAsync(ct);
            return NoContent();
        }
    }
}
