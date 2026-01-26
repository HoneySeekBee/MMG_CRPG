using Application.Skills;
using Contracts.Protos;
using Google.Protobuf;
using Microsoft.AspNetCore.Mvc;
namespace Presentation.Controllers.Game
{
    [ApiController]
    [Route("api/pb/status")]
    [Produces("application/x-protobuf")]
    public sealed class GameStatusController : ControllerBase
    {
        private readonly ILogger<GameStatusController> _logger;


        public GameStatusController(ILogger<GameStatusController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var res = new StatusPb
            {
                Maintenance = false,
                ForceUpdate = false,
                Message = "상태에 대해서 알려줄게요",
                ServerUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };
            _logger.LogInformation($"[StatusReq] - 누군가 서버에 상태 확인 {res.Message} : ");
            return File(res.ToByteArray(), "application/x-protobuf");
        }
    }
}
