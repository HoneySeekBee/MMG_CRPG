using Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Combat
{
    public sealed class FakeMasterDataProvider : IMasterDataProvider
    {
        public Task<Domain.Services.MasterDataPack> BuildPackAsync(
            int stageId, IReadOnlyCollection<long> partyCharacterIds, CancellationToken ct)
        {
            // 임시 적 3명, 고정 스탯
            var stage = new Domain.Services.StageDef(
                stageId,
                new[]
                {
                new Domain.Services.EnemySpawn(1001, 10),
                new Domain.Services.EnemySpawn(1002, 10),
                new Domain.Services.EnemySpawn(1003, 10)
                }
            );

            // 아군/적 공통으로 쓸 간단 스탯
            var charIds = partyCharacterIds.Concat(new long[] { 1001, 1002, 1003 }).Distinct();
            var chars = charIds.ToDictionary(
                id => id,
                id => new Domain.Services.CharacterDef(
                    id, BaseHp: 1000, BaseAtk: 120, BaseDef: 80, BaseAspd: 1200, CritRate: 0.1f, CritDmg: 0.5f
                )
            );
            var skills = new Dictionary<long, SkillDef>(); // 비워둬도 엔진이 기본공격만으로 동작

            var pack = new Domain.Services.MasterDataPack(stage, chars, skills);
            return Task.FromResult(pack);
        }
    }

}
