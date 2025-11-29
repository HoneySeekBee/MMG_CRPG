 
using Application.Repositories;
using Application.Swagger.WebServer.DemoDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("api/test/gacha")]
    [Produces("application/json")]
    public class TestGachaController : ControllerBase
    {
        private readonly IGachaPoolRepository _repo;
        public TestGachaController(IGachaPoolRepository repo) => _repo = repo;

        [HttpPost("roll")]
        [Consumes("application/json")]
        [AllowAnonymous] // 데모용
        public async Task<ActionResult<GachaRollResult>> Roll([FromBody] GachaRollRequest req, CancellationToken ct)
        {
            if (req.Count <= 0) return BadRequest("count must be positive");

            // 풀 + 엔트리 로드 (Entries 포함)
            var pool = await _repo.GetWithEntriesAsync(req.PoolId, ct)
                       ?? await _repo.GetByIdAsync(req.PoolId, ct);
            if (pool is null) return NotFound($"Pool {req.PoolId} not found");

            var entries = pool.Entries?.ToList() ?? new();
            if (entries.Count == 0) return BadRequest("Pool has no entries");

            // 가중치 합
            var totalWeight = entries.Sum(e => e.Weight);
            if (totalWeight <= 0) return BadRequest("Invalid weights");

            // 보안 난수(충분)
            List<int> resultIds = new(req.Count);
            for (int i = 0; i < req.Count; i++)
            {
                var roll = RandomNumberGenerator.GetInt32(1, totalWeight + 1); // 1..total
                int acc = 0;
                foreach (var e in entries)
                {
                    acc += e.Weight;
                    if (roll <= acc)
                    {
                        resultIds.Add(e.CharacterId);
                        break;
                    }
                }
            }

            return Ok(new GachaRollResult(req.PoolId, resultIds));
        }
    }
}
