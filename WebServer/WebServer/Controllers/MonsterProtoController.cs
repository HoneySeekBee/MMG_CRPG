using Application.Monsters;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using WebServer.Protos.Monsters;

namespace WebServer.Controllers
{ 
    [ApiController]
    [Route("api/pb/monsters")]
    [Produces("application/x-protobuf")] 
    public sealed class MonsterProtoController : ControllerBase
    {
        private readonly IMonsterCache _cache;
        public MonsterProtoController(IMonsterCache cache) => _cache = cache;
        [HttpGet]
        public IActionResult GetAll()
        {
            var monsters = _cache.GetAll().Select(ToPb).ToList();
            var resp = new MonsterListResponsePb
            {
                Version = DateTimeOffset.UtcNow.ToUnixTimeSeconds(), // 캐시 버전용
            };
            resp.Monsters.AddRange(monsters);
            return File(resp.ToByteArray(), "application/x-protobuf");
        }


        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            var dto = _cache.GetById(id);
            if (dto is null) return NotFound();
            var pb = ToPb(dto);
            return File(pb.ToByteArray(), "application/x-protobuf");
        }

        // (선택) 캐시 리로드 엔드포인트
        [HttpPost("reload")]
        public async Task<IActionResult> Reload(CancellationToken ct)
        {
            await _cache.ReloadAsync(ct);
            var count = _cache.GetAll().Count;
            var pb = new ReloadResultPb { Count = count };
            return File(pb.ToByteArray(), "application/x-protobuf");
        }

        // --- mapping ---
        private static MonsterPb ToPb(MonsterDto d)
        {
            var pb = new MonsterPb
            {
                Id = d.Id,
                Name = d.Name,
                ModelKey = d.ModelKey
            };

            if (d.ElementId.HasValue) pb.ElementId = d.ElementId.Value;
            if (d.PortraitId.HasValue) pb.PortraitId = d.PortraitId.Value;

            pb.Stats.AddRange(d.Stats.Select(ToPb));
            return pb;
        }

        private static MonsterStatPb ToPb(MonsterStatDto s) => new MonsterStatPb
        {
            MonsterId = s.MonsterId,
            Level = s.Level,
            Hp = s.HP,
            Atk = s.ATK,
            Def = s.DEF,
            Spd = s.SPD,
            CritRate = (double)s.CritRate,
            CritDamage = (double)s.CritDamage
        };
    }
}
