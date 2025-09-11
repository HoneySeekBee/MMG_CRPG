using Contracts.Protos;
using Microsoft.AspNetCore.Mvc;
namespace WebServer.Controllers
{
    [ApiController]
    [Route("api/pb/status")]
    [Produces("application/x-protobuf")]
    public sealed class StatusProtoController : ControllerBase
    {
        [HttpGet]
        public ActionResult<Status> Get()
        {
            // 실제 운영 로직 연결 전, 최소 형태
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            return Ok(new Status
            {
                Maintenance = false,
                ForceUpdate = false,
                Message = "",
                ServerUnixMs = now
            });
        }
    }
}
