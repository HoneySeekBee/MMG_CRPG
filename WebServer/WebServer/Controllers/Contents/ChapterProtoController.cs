using Application.Contents.Chapters;
using Google.Protobuf;
using Microsoft.AspNetCore.Mvc;
using WebServer.Mappers.Contents;

namespace WebServer.Controllers.Contents
{
    [ApiController]
    [Route("api/pb/[controller]")]
    public class ChaptersProtoController : ControllerBase
    {
        private readonly IChapterCache _chapterCache;

        public ChaptersProtoController(IChapterCache chapterCache)
        {
            _chapterCache = chapterCache;
        }

        // GET: api/proto/chapters
        [HttpGet]
        public IActionResult GetAll()
        {
            var list = _chapterCache.GetAll();
            var proto = ChapterProtoMapper.ToProto(list);
            var bytes = proto.ToByteArray();
            return File(bytes, "application/x-protobuf");
        }

        // GET: api/proto/chapters/5
        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            var dto = _chapterCache.GetById(id);
            if (dto is null)
                return NotFound();

            var proto = ChapterProtoMapper.ToProto(dto);
            var bytes = proto.ToByteArray();
            return File(bytes, "application/x-protobuf");
        }

        // 필요하면 캐시 리로드
        [HttpPost("reload")]
        public async Task<IActionResult> Reload(CancellationToken ct)
        {
            await _chapterCache.ReloadAsync(ct);
            return NoContent();
        }
    }
}
