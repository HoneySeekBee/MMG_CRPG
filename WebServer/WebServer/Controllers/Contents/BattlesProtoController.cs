using Application.Contents.Battles;
using Google.Protobuf;
using Microsoft.AspNetCore.Mvc;
using WebServer.Mappers.Contents;

namespace WebServer.Controllers.Contents
{
    [ApiController]
    [Route("api/pb/[controller]")]
    public class BattlesProtoController : ControllerBase
    {
        private readonly IBattlesCache _battlesCache;

        public BattlesProtoController(IBattlesCache battlesCache)
        {
            _battlesCache = battlesCache;
        }

        // GET: api/proto/battles
        [HttpGet]
        public IActionResult GetAll()
        {
            var list = _battlesCache.GetAll();
            var proto = BattleProtoMapper.ToProto(list);

            // 직렬화
            var bytes = proto.ToByteArray();
            return File(bytes, "application/x-protobuf");
        }

        // GET: api/proto/battles/5
        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            var dto = _battlesCache.GetById(id);
            if (dto is null)
                return NotFound();

            var proto = BattleProtoMapper.ToProto(dto);
            var bytes = proto.ToByteArray();
            return File(bytes, "application/x-protobuf");
        }
    }
}
