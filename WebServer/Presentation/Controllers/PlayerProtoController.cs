using Contracts.Protos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("api/pb/player")]
    [Produces("application/x-protobuf")]
    public sealed class PlayerProtoController : ControllerBase
    {
        // 필요한 서비스 주입
        // private readonly IPlayerQueryService _players;
        // public PlayerProtoController(IPlayerQueryService players) => _players = players;

        [Authorize]
        [HttpGet("bootstrap")]
        public ActionResult<PlayerBootstrap> Bootstrap()
        {
            // TODO: 유저 ID는 User.Claims에서 꺼내거나, IUserContext 등을 통해 조회
            var nickname = "Hero";
            var soft = 1234;
            var hard = 56;
            var banners = new[] { "Welcome Banner", "Launch Gacha" };

            return Ok(new PlayerBootstrap
            {
                Nickname = nickname,
                SoftCurrency = soft,
                HardCurrency = hard,
                BannerSummaries = { banners },
                ServerUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            });
        }
    }
}
