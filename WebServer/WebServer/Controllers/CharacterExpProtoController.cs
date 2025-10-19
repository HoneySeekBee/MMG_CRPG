using Application.Character;
using Google.Protobuf;
using Microsoft.AspNetCore.Mvc;
using WebServer.Protos;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("api/pb/character-exp")]
    [Produces("application/x-protobuf")]
    public class CharacterExpProtoController : ControllerBase
    {
        private readonly ICharacterExpCache _cache;

        public CharacterExpProtoController(ICharacterExpCache cache)
        {
            _cache = cache;
        }

        [HttpGet("grouped")]
        public IActionResult GetGrouped()
        {
            var all = _cache.GetAll();

            var resp = new CharacterExpTableResponse
            {
                Version = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            // rarityId별로 묶어서 그룹 생성
            var groups = all
                .GroupBy(x => x.RarityId)
                .OrderBy(g => g.Key);

            foreach (var g in groups)
            {
                var group = new CharacterExpGroupPb
                {
                    RarityId = g.Key
                };

                foreach (var lvl in g.OrderBy(x => x.Level))
                {
                    group.Levels.Add(new CharacterExpLevelPb
                    {
                        Level = lvl.Level,
                        RequiredExp = lvl.RequiredExp
                    });
                }

                resp.Groups.Add(group);
            }

            return File(resp.ToByteArray(), "application/x-protobuf");
        }

        [HttpGet("flat")]
        public IActionResult GetFlat()
        {
            var all = _cache.GetAll();

            var resp = new CharacterExpFlatResponse
            {
                Version = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            foreach (var row in all)
            {
                resp.Rows.Add(new CharacterExpRowPb
                {
                    RarityId = row.RarityId,
                    Level = row.Level,
                    RequiredExp = row.RequiredExp
                });
            }

            return File(resp.ToByteArray(), "application/x-protobuf");
        }

        [HttpGet("{rarityId:int}/{level:int}")]
        public IActionResult GetOne(int rarityId, int level)
        {
            var row = _cache.Get(rarityId, (short)level);
            if (row is null) return NotFound();

            var resp = new CharacterExpFlatResponse
            {
                Version = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            resp.Rows.Add(new CharacterExpRowPb
            {
                RarityId = row.RarityId,
                Level = row.Level,
                RequiredExp = row.RequiredExp
            });

            return File(resp.ToByteArray(), "application/x-protobuf");
        }
    }
}
