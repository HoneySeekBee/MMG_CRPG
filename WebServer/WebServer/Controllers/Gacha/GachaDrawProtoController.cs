using Application.Gacha.GachaDraw;
using Contracts.Protos;
using Google.Protobuf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProtoBuf;

namespace WebServer.Controllers.Gacha
{
    [ApiController]
    [Authorize]
    [Route("api/pb/gacha")]
    [Authorize]
    [Produces("application/x-protobuf")]
    public sealed class GachaDrawProtoController : ControllerBase
    {
        private readonly IGachaDrawService _drawService;

        public GachaDrawProtoController(IGachaDrawService drawService)
        {
            _drawService = drawService;
        }
        [HttpPost("draw")]
        public async Task<IActionResult> Draw(CancellationToken ct)
        {
            // 1) protobuf 요청 파싱
            using var ms = new MemoryStream();
            await Request.Body.CopyToAsync(ms, ct);

            var req = GachaDrawRequestPb.Parser.ParseFrom(ms.ToArray()); 
            // 2) UserId (JWT)
            int userId = int.Parse(User.FindFirst("uid")!.Value);

            // 3) Application 호출
            var result = await _drawService.DrawAsync(
                req.BannerKey,
                req.Count,
                userId,
                ct);

            // 4) 결과 → Protobuf Response
            var pb = new GachaDrawResultPb
            {
                TimestampUtc = result.Timestamp.ToUnixTimeSeconds(),
                UsedTickets = result.UsedTickets,
                UsedCurrency = result.UsedCurrency,
                TotalCharacters = result.TotalCharacters,
                TotalShards = result.TotalShards
            };

            foreach (var item in result.Items)
            {
                pb.Items.Add(new GachaDrawItemPb
                {
                    CharacterId = item.CharacterId,
                    Grade = item.Grade,
                    RateUp = item.RateUp,
                    IsNew = item.IsNew,
                    IsShard = item.IsShard,
                    ShardAmount = item.ShardAmount,
                    IsGuaranteed = item.IsGuaranteed
                });
            }

            // 5) protobuf 직렬화하여 반환
            var outStream = new MemoryStream();
            pb.WriteTo(outStream);  

            return File(outStream.ToArray(), "application/x-protobuf");
        }
    }
}
