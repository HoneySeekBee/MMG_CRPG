using Application.EquipSlots;
using Contracts.EquipSlots;
using Google.Protobuf;
using Microsoft.AspNetCore.Mvc;
using WebServer.Mappers;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("api/pb/equipslots")]
    [Produces("application/x-protobuf")]
    public class EquipSlotsProtoController : ControllerBase
    {
        private readonly IEquipSlotCache _cache;
        public EquipSlotsProtoController(IEquipSlotCache cache)
        {
            _cache = cache;
        }
        private FileContentResult Proto(IMessage msg)
        {
            using var ms = new MemoryStream();
            msg.WriteTo(ms);
            return File(ms.ToArray(), "application/x-protobuf");
        }

        [HttpGet]
        [Produces("application/x-protobuf", "application/json")]
        public IActionResult GetAll([FromQuery] string? format = null)
        {
            var listPb = _cache.GetAll().ToProtoList();

            if (string.Equals(format, "json", StringComparison.OrdinalIgnoreCase))
                return Ok(listPb);

            return Proto(listPb);
        }

        [HttpGet("{id:int}")]
        [Produces("application/x-protobuf", "application/json")]
        public IActionResult GetById(int id, [FromQuery] string? format = null)
        {
            var dto = _cache.GetById(id);
            if (dto is null) return NotFound();

            var pb = dto.ToProto();

            if (string.Equals(format, "json", StringComparison.OrdinalIgnoreCase))
                return Ok(pb);

            return Proto(pb);
        }

    }
}
