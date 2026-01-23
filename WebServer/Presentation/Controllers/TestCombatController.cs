using Application.Combat;
using Application.Swagger.WebServer.DemoDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("api/test/combat")]
    [Produces("application/json")]
    public class TestCombatController : ControllerBase
    {
        [HttpPost("simulate")]
        [Consumes("application/json")]
        [AllowAnonymous] // 데모용
        public ActionResult<CombatSimResult> Simulate([FromBody] CombatSimRequest req)
        {
            if (req.Allies is null || req.Enemies is null || req.Allies.Count == 0 || req.Enemies.Count == 0)
                return BadRequest("Need at least 1 ally and 1 enemy");

            // ---- 데모 규칙 (간단/결정적) ----
            // 각 유닛의 "전투력" = characterId % 100 + level*10
            // 총합이 큰 쪽이 승리. 동률이면 allies 승.
            int Score(IEnumerable<CombatUnit> units) =>
                units.Sum(u => (u.CharacterId % 100) + (u.Level * 10));

            int sAllies = Score(req.Allies);
            int sEnemies = Score(req.Enemies);

            string winner = sAllies >= sEnemies ? "Allies" : "Enemies";
            var steps = new List<string>
            {
                $"Allies score = {sAllies}",
                $"Enemies score = {sEnemies}",
                $"{winner} win!"
            };

            return Ok(new CombatSimResult(
                Winner: winner,
                Log: new CombatLog("Demo combat resolved by simple score rule.", steps)
            ));
        }
    }
}
