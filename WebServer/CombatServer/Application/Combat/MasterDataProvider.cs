using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Combat;
using Application.Combat.Engine;
using Application.UserCharacter;
using Domain.Combat.Engine;
using Domain.Entities.Combats;

namespace Application.Combat
{
    public sealed class MasterDataProvider : IMasterDataProvider
    {
        private readonly IStageReader _stage;
        private readonly ICharacterReader _char;
        private readonly ISkillReader _skill;
        private readonly IUserCharacterReader _userChars;
        private readonly IMonsterStatReader _monsterStats;

        // Default crit values (not in DTO, apply as baseline)
        private const float DefaultCritRate = 0.10f; // 10%
        private const float DefaultCritDmg = 0.50f; // +50%

        // Default enemy level (used when stage has none)
        private const int DefaultEnemyLevel = 1;

        public MasterDataProvider(IStageReader stage, ICharacterReader @char, ISkillReader skill, IUserCharacterReader userChars, IMonsterStatReader monsterStats)
        {
            _stage = stage;
            _char = @char;
            _skill = skill;
            _userChars = userChars;
            _monsterStats = monsterStats
                ?? throw new ArgumentNullException(nameof(monsterStats));
        }
        public async Task<Domain.Services.MasterDataPack> BuildEnginePackAsync(
           int stageId,
           IReadOnlyCollection<long> partyCharacterIds,
           CancellationToken ct)
        {
            // 1) Load stage detail (StageDetailDto)
            var stage = await _stage.GetAsync(stageId, ct)
                       ?? throw new KeyNotFoundException($"Stage {stageId} not found");

            // 2) Build domain StageDef.Enemies
            var enemySpawns = stage.Waves
                .OrderBy(w => w.Index)
                .SelectMany(w => w.Enemies)
                .Select(e => new Domain.Services.EnemySpawn(
                    CharacterId: (long)e.EnemyCharacterId,
                    Level: e.Level == 0 ? DefaultEnemyLevel : e.Level
                ))
                .ToList();

            var stageDef = new Domain.Services.StageDef(
                StageId: stage.Id,
                Enemies: enemySpawns
            );

            // 3) All character IDs (player + enemy)
            var enemyIds = enemySpawns
                .Select(e => e.CharacterId)
                .Distinct();

            var allCharIds = enemyIds
                .Concat(partyCharacterIds)
                .Distinct()
                .ToArray();

            // 4) Character/skill master -> Domain.Services.*
            var chars = await LoadCharactersAsync(allCharIds, ct);

            var skills = await LoadSkillsAsync(Array.Empty<long>(), ct);

            // 5) Return Domain.Services.MasterDataPack
            return new Domain.Services.MasterDataPack(
                Stage: stageDef,
                Characters: chars,
                Skills: skills
            );
        }

        public async Task<MasterPack> BuildPackAsync(int stageId, long userId, IReadOnlyCollection<long> partyCharacterIds, CancellationToken ct)
        {
            // 1) Load stage detail (StageDetailDto)
            var stage = await _stage.GetAsync(stageId, ct)
                       ?? throw new KeyNotFoundException($"Stage {stageId} not found");

            // 2) Build CombatStageDef.Waves
            var waveDefs = stage.Waves
                .OrderBy(w => w.Index)
                .Select(w => new CombatWaveDef(
                    index: w.Index,
                    enemies: w.Enemies
                        .OrderBy(e => e.Slot)
                        .Select(e => new CombatEnemySpawn(
                            slot: e.Slot,
                            monsterId: e.EnemyCharacterId,
                            level: e.Level
                        ))
                        .ToList()
                ))
                .ToList();

            var stageDef = new CombatStageDef(
                stageId: stage.Id,
                waves: waveDefs
            );

            // 2) Load user character stats
            var userStats = await _userChars.GetManyByCharacterIdAsync(
                partyCharacterIds,
                userId,
                ct
            );

            var userStatsById = userStats.ToDictionary(x => (long)x.CharacterId);

            // 3) Build ActorDef dictionary
            var actors = new Dictionary<long, CombatActorDef>();

            // 3-1) Player units
            foreach (var us in userStats)
            {
                int attackIntervalMs = (int)(1000f / MathF.Sqrt(us.Spd));
                if (attackIntervalMs < 200) attackIntervalMs = 200;
                var def = new CombatActorDef(
                    masterId: us.CharacterId,
                    isPlayer: true,
                    modelKey: $"Hero_{us.CharacterId}",
                    maxHp: us.Hp,
                    atk: us.Atk,
                    def: us.Def,
                    spd: us.Spd,
                    range: us.Range,
                    attackIntervalMs: attackIntervalMs,
                    critRate: us.CritRate,
                    critDamage: us.CritDamage
                );

                actors[us.CharacterId] = def;
            }

            // 3-2) Enemy units (monsters)
            var enemyIds = waveDefs
                .SelectMany(w => w.Enemies)
                .Select(e => (long)e.MonsterId)
                .Distinct();

            foreach (var mid in enemyIds)
            {
                if (actors.ContainsKey(mid))
                    continue;
                var firstSpawn = waveDefs
                    .SelectMany(w => w.Enemies)
                    .First(e => e.MonsterId == mid);
                int level = firstSpawn.Level == 0 ? DefaultEnemyLevel : firstSpawn.Level;

                var m = await _monsterStats.GetAsync(mid, level, ct)
                        ?? throw new KeyNotFoundException($"MonsterStat {mid} Lv{level} not found");

                int attackIntervalMs = (int)(1000f / MathF.Sqrt(m.SPD));
                if (attackIntervalMs < 200) attackIntervalMs = 200;
                var def = new CombatActorDef(
                    masterId: (int)mid,
                    isPlayer: false,
                    modelKey: $"Enemy_{mid}",
                    maxHp: m.HP,
                    atk: m.ATK,
                    def: m.DEF,
                    spd: m.SPD,
                    range: m.Range,
                    attackIntervalMs: attackIntervalMs,
                    critRate: (double)m.CritRate,
                    critDamage: (double)m.CritDamage
                );

                actors[mid] = def;
            }

            return new MasterPack(stageDef, actors);
        }

        private async Task<IReadOnlyDictionary<long, Domain.Services.CharacterDef>> LoadCharactersAsync(
            IReadOnlyCollection<long> ids, CancellationToken ct)
        {
            var result = new Dictionary<long, Domain.Services.CharacterDef>(ids.Count);
            foreach (var id in ids)
            {
                var c = await _char.GetAsync(id, ct)
                        ?? throw new KeyNotFoundException($"character {id} not found");

                result[id] = new Domain.Services.CharacterDef(
                    CharacterId: c.CharacterId,
                    BaseHp: c.BaseHp,
                    BaseAtk: c.BaseAtk,
                    BaseDef: c.BaseDef,
                    BaseAspd: c.BaseAspd,
                    CritRate: DefaultCritRate,
                    CritDmg: DefaultCritDmg
                );
            }
            return result;
        }

        private async Task<IReadOnlyDictionary<long, Domain.Services.SkillDef>> LoadSkillsAsync(
            IReadOnlyCollection<long> ids, CancellationToken ct)
        {
            if (ids.Count == 0) return new Dictionary<long, Domain.Services.SkillDef>();
            var dict = new Dictionary<long, Domain.Services.SkillDef>(ids.Count);

            foreach (var id in ids)
            {
                var s = await _skill.GetAsync(id, ct);
                if (s == null) continue;
                dict[id] = new Domain.Services.SkillDef(s.SkillId, s.CooldownMs, s.Coeff);
            }
            return dict;
        }
    }
}
