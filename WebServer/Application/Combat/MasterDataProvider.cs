using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Combat;
using Application.Combat.Engine;
using Application.UserCharacter;

namespace Application.Combat
{
    public sealed class MasterDataProvider : IMasterDataProvider
    {
        private readonly IStageReader _stage;
        private readonly ICharacterReader _char;
        private readonly ISkillReader _skill;
        private readonly IUserCharacterReader _userChars;
        private readonly IMonsterStatReader _monsterStats;

        // 크리 기본값 (DTO에 없으므로 임시 적용)
        private const float DefaultCritRate = 0.10f; // 10%
        private const float DefaultCritDmg = 0.50f; // +50%

        // 적 레벨 기본값 (Stage에 없으면 사용)
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
            // 1) 스테이지 상세 로드 (StageDetailDto)
            var stage = await _stage.GetAsync(stageId, ct)
                       ?? throw new KeyNotFoundException($"Stage {stageId} not found");

            // 2) 도메인 StageDef.Enemies 구성
            //    - StageDetailDto.Waves[].Enemies[] 를 flat하게 펼쳐서 EnemySpawn 리스트로 만든다.
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

            // 3) 등장하는 모든 캐릭터 ID (유저 + 적)
            var enemyIds = enemySpawns
                .Select(e => e.CharacterId) // long
                .Distinct();

            var allCharIds = enemyIds
                .Concat(partyCharacterIds)
                .Distinct()
                .ToArray();

            // 4) 캐릭터/스킬 마스터 → Domain.Services.* 로 매핑
            var chars = await LoadCharactersAsync(allCharIds, ct);

            // 스킬은 아직 안 쓰니까 일단 빈 배열 전달하거나, 
            var skills = await LoadSkillsAsync(Array.Empty<long>(), ct);

            // 5) Domain.Services.MasterDataPack 반환
            return new Domain.Services.MasterDataPack(
                Stage: stageDef,
                Characters: chars,
                Skills: skills
            );
        }

        public async Task<MasterPackDto> BuildPackAsync(int stageId, long userId, IReadOnlyCollection<long> partyCharacterIds, CancellationToken ct)
        {
            // 1) 스테이지 상세 정보 로드 (StageDetailDto)
            var stage = await _stage.GetAsync(stageId, ct)
                       ?? throw new KeyNotFoundException($"Stage {stageId} not found");

            // 2) CombatStageDef.Waves 구성 (WaveDto, EnemyDto -> CombatWaveDef, CombatEnemySpawn)
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
            // 2) 유저 캐릭터 스탯 한번에 로드
            var userStats = await _userChars.GetManyByCharacterIdAsync(
      partyCharacterIds,
      userId,   // 또는 req.UserId 전달해야 함
      ct
  );

            var userStatsById = userStats.ToDictionary(x => (long)x.CharacterId);

            // 3) ActorDef 딕셔너리 채운다
            var actors = new Dictionary<long, CombatActorDef>();

            // 3-1) 플레이어 유닛들
            foreach (var us in userStats)
            {
                int attackIntervalMs = (int)(1000f / MathF.Sqrt(us.Spd));
                if (attackIntervalMs < 200) attackIntervalMs = 200;
                var def = new CombatActorDef(
                    masterId: us.CharacterId,      // 마스터 캐릭터 id
                    isPlayer: true,
                    modelKey: $"Hero_{us.CharacterId}",
                    maxHp: us.Hp,
                    atk: us.Atk,
                    def: us.Def,
                    spd: us.Spd,
                    range: us.Range,
                    attackIntervalMs: attackIntervalMs,      // 일단 Aspd = Spd 그대로 사용
                    critRate: us.CritRate,
                    critDamage: us.CritDamage
                );

                // key 에 무엇을 쓸지는 네가 정하면 되는데,
                // 지금 StartAsync 에서는 slot.UserCharacterId 를 쓰고 있으니
                // 여기서는 UserCharacterId 를 key 로 두는게 자연스럽다
                actors[us.CharacterId] = def;
            }

            // 3-2) 적 유닛들 (몬스터) — 당장은 더미 값으로
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

            return new MasterPackDto(stageDef, actors);
        }


        private async Task<IReadOnlyDictionary<long, CombatActorDef>> LoadActorDefsAsync(
            IReadOnlyCollection<long> allCharIds,
            IReadOnlyCollection<long> partyCharacterIds,
            CancellationToken ct)
        {
            var dict = new Dictionary<long, CombatActorDef>(allCharIds.Count);

            foreach (var id in allCharIds)
            {
                var c = await _char.GetAsync(id, ct)
                        ?? throw new KeyNotFoundException($"character {id} not found");

                bool isPlayer = partyCharacterIds.Contains(id);

                // CharacterMasterDto: (CharacterId, BaseHp, BaseAtk, BaseDef, BaseAspd, ...)
                // ModelKey / Range / AttackIntervalMs / Crit 정보는 네 마스터 구조에 맞게 채워줘야 함.
                var modelKey = isPlayer ? $"Hero_{id}" : $"Enemy_{id}";
                var range = 1.5f;           // TODO: 마스터에 필드 있으면 그거 사용
                var attackIntervalMs = 1000; // TODO: Aspd로부터 계산하거나 DTO 필드 사용

                var def = new CombatActorDef(
                    masterId: (int)c.CharacterId,
                    isPlayer: isPlayer,
                    modelKey: modelKey,
                    maxHp: c.BaseHp,
                    atk: c.BaseAtk,
                    def: c.BaseDef,
                    spd: c.BaseAspd,
                    range: range,
                    attackIntervalMs: attackIntervalMs,
                    critRate: DefaultCritRate,
                    critDamage: DefaultCritDmg
                );

                dict[c.CharacterId] = def;
            }

            return dict;
        }
        private async Task<IReadOnlyDictionary<long, Domain.Services.CharacterDef>> LoadCharactersAsync(
            IReadOnlyCollection<long> ids, CancellationToken ct)
        {
            // 필요한 만큼만 로드
            var result = new Dictionary<long, Domain.Services.CharacterDef>(ids.Count);
            foreach (var id in ids)
            {
                var c = await _char.GetAsync(id, ct)
                        ?? throw new KeyNotFoundException($"character {id} not found");

                // CritRate / CritDmg 이 DTO에 없으므로 기본값 사용
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
                if (s == null) continue; // MVP: 없으면 스킵
                dict[id] = new Domain.Services.SkillDef(s.SkillId, s.CooldownMs, s.Coeff);
            }
            return dict;
        }
    }
}
