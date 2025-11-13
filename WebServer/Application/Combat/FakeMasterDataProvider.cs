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
        public Task<MasterDataPack> BuildEnginePackAsync(
            int stageId,
            IReadOnlyCollection<long> partyCharacterIds,
            CancellationToken ct)
        {
            // 임시 적 3명, 고정 스탯
            var stage = new StageDef(
                stageId,
                new[]
                {
                    new EnemySpawn(1001, 10),
                    new EnemySpawn(1002, 10),
                    new EnemySpawn(1003, 10)
                }
            );

            // 아군/적 공통으로 쓸 간단 스탯
            var charIds = partyCharacterIds
                .Concat(new long[] { 1001, 1002, 1003 })
                .Distinct();

            var chars = charIds.ToDictionary(
                id => id,
                id => new CharacterDef(
                    CharacterId: id,
                    BaseHp: 1000,
                    BaseAtk: 120,
                    BaseDef: 80,
                    BaseAspd: 1200,
                    CritRate: 0.1f,
                    CritDmg: 0.5f
                )
            );

            // 스킬은 비워둬도 엔진이 기본공격만으로 동작
            var skills = new Dictionary<long, SkillDef>();

            var pack = new MasterDataPack(stage, chars, skills);
            return Task.FromResult(pack);
        }
        public Task<CombatMasterDataPack> BuildPackAsync(
             int stageId,
             long userId,
             IReadOnlyCollection<long> partyCharacterIds,
             CancellationToken ct)
        {
            // 1) CombatStageDef : 웨이브 1개, 적 3마리
            var wave = new CombatWaveDef(
                index: 1,
                enemies: new List<CombatEnemySpawn>
                {
                    new CombatEnemySpawn(slot: 1, monsterId: 1001, level: 10),
                    new CombatEnemySpawn(slot: 2, monsterId: 1002, level: 10),
                    new CombatEnemySpawn(slot: 3, monsterId: 1003, level: 10),
                }
            );

            var stageDef = new CombatStageDef(
                stageId: stageId,
                waves: new List<CombatWaveDef> { wave }
            );

            // 2) 아군 + 적 캐릭터 모두 모으기
            var allCharIds = partyCharacterIds
                .Concat(new long[] { 1001, 1002, 1003 })
                .Distinct()
                .ToArray();

            var actors = new Dictionary<long, CombatActorDef>(allCharIds.Length);

            foreach (var id in allCharIds)
            {
                bool isPlayer = partyCharacterIds.Contains(id);
                var modelKey = isPlayer ? $"Hero_{id}" : $"Enemy_{id}";

                // 대충 통일 스탯
                var def = new CombatActorDef(
                    masterId: (int)id,      // CombatActorDef가 int라면 캐스팅
                    isPlayer: isPlayer,
                    modelKey: modelKey,
                    maxHp: 1000,
                    atk: 120,
                    def: 80,
                    spd: 1200,
                    range: 1.5f,
                    attackIntervalMs: 1000,
                    critRate: 0.1,
                    critDamage: 0.5
                );

                actors[id] = def;
            }

            var combatPack = new CombatMasterDataPack(stageDef, actors);
            return Task.FromResult(combatPack);
        }
    }

}
