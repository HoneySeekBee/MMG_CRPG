using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Swagger
{
    namespace WebServer.DemoDtos
    {
        public record GachaRollRequest(int PoolId, int Count = 10);
        public record GachaRollResult(int PoolId, IReadOnlyList<int> CharacterIds);

        public record CombatUnit(int CharacterId, int Level);
        public record CombatSimRequest(IReadOnlyList<CombatUnit> Allies, IReadOnlyList<CombatUnit> Enemies);

        public record CombatLog(string Summary, IReadOnlyList<string> Steps);
        public record CombatSimResult(string Winner, CombatLog Log);
    }
}
