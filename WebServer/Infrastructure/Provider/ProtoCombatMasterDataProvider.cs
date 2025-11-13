using Application.Combat;
using Contracts.Protos;
using Infrastructure.Reader; 
using WebServer.Protos;
using WebServer.Protos.Monsters;
using Application.Combat.Engine;

namespace Infrastructure.Provider
{
    public sealed class ProtoCombatMasterDataProvider : ICombatMasterDataProvider
    {
        private readonly IStageAssetReader _stageReader;
        private readonly IMonsterStatReader _monsterReader;
        private readonly ICharacterAssetReader _characterReader;
        private readonly IRangeConfigReader _rangeReader;

        // 기본값들
        private const float DefaultMeleeRange = 1.0f;
        private const float DefaultRangedRange = 3.0f;
        private const int DefaultMonsterAttackIntervalMs = 1500;
        private const int DefaultPlayerAttackIntervalMs = 1000;

        public ProtoCombatMasterDataProvider(
            IStageAssetReader stageReader,
            IMonsterStatReader monsterReader,
            ICharacterAssetReader characterReader,
            IRangeConfigReader rangeReader)
        {
            _stageReader = stageReader;
            _monsterReader = monsterReader;
            _characterReader = characterReader;
            _rangeReader = rangeReader;
        }

        public async Task<CombatMasterDataPack> BuildPackAsync(
            int stageId,
            IReadOnlyCollection<long> playerCharacterIds,
            CancellationToken ct)
        {
            // 1) StagePb 로드
            var stagePb = await _stageReader.GetStageAsync(stageId, ct)
                          ?? throw new KeyNotFoundException($"Stage {stageId} not found.");

            // 2) StagePb → CombatStageDef
            var stageDef = MapStage(stagePb);

            // 3) 웨이브들에서 등장하는 몬스터 ID 목록
            var enemyMonsterIds = stageDef.Waves
                .SelectMany(w => w.Enemies)
                .Select(e => e.MonsterId)
                .Distinct()
                .ToArray();

            // 4) 마스터 몬스터/캐릭터 로드
            var monsterDict = new Dictionary<long, CombatActorDef>();

            foreach (var spawn in stageDef.Waves.SelectMany(w => w.Enemies))
            {
                var stat = await _monsterReader.GetAsync(spawn.MonsterId, spawn.Level, ct);
                if (stat == null) continue;

                monsterDict[spawn.MonsterId] = new CombatActorDef(
                    masterId: stat.MonsterId,
                    isPlayer: false,
                    modelKey: $"Monster_{stat.MonsterId}",
                    maxHp: stat.HP,
                    atk: stat.ATK,
                    def: stat.DEF,
                    spd: stat.SPD,
                    range: stat.Range,
                    attackIntervalMs: stat.SPD,
                    critRate: (double)stat.CritRate,
                    critDamage: (double)stat.CritDamage
                );
            }
            var characterDict = playerCharacterIds.Count > 0
                ? await _characterReader.GetCharactersAsync(playerCharacterIds, ct)
                : new Dictionary<long, CharacterDetailPb>();

            // 5) CombatActorDef 구성
            var actors = new Dictionary<long, CombatActorDef>();

            // 5-1) 몬스터
            foreach (var monsterId in enemyMonsterIds)
            {
                if (!monsterDict.TryGetValue(monsterId, out var def))
                    throw new KeyNotFoundException($"Monster {monsterId} not found.");

                // 이미 def 가 CombatActorDef 라서 바로 넣으면 됨
                actors[def.MasterId] = def;
            }

            // 5-2) 플레이어 캐릭터
            foreach (var charId in playerCharacterIds)
            {
                if (!characterDict.TryGetValue(charId, out var ch))
                    throw new KeyNotFoundException($"Character {charId} not found.");

                // TODO: 실제 유저 데이터에서 level/tier 불러오기
                var def = await MapCharacterToCombatActorDefAsync(
                    ch, level: 1, tier: 1, ct);

                actors[def.MasterId] = def;
            }

            return new CombatMasterDataPack(stageDef, actors);
        }

        // StagePb → CombatStageDef
        private static CombatStageDef MapStage(StagePb pb)
        {
            var waves = pb.Waves
                .OrderBy(w => w.Index)
                .Select(w =>
                {
                    var spawns = w.Enemies
                        .Select(e => new CombatEnemySpawn(
                            slot: e.Slot,
                            monsterId: e.EnemyCharacterId,
                            level: e.Level
                        ))
                        .ToList();

                    return new CombatWaveDef(w.Index, spawns);
                })
                .ToList();

            return new CombatStageDef(pb.Id, waves);
        }

        // MonsterPb → CombatActorDef
        private async Task<CombatActorDef> MapMonsterToCombatActorDefAsync(
            MonsterPb monster,
            int level,
            CancellationToken ct)
        {
            var stat = monster.Stats
                .OrderBy(s => s.Level)
                .LastOrDefault(s => s.Level <= level)
                       ?? monster.Stats.OrderBy(s => s.Level).First();

            // 사거리: DB/설정 테이블에서 우선 가져오고, 없으면 기본값 사용
            var range = await _rangeReader.GetRangeAsync(monster.Id, isPlayer: false, ct)
                        ?? GuessRangeForMonster(monster);

            return new CombatActorDef(
                masterId: monster.Id,
                isPlayer: false,
                modelKey: monster.ModelKey,
                maxHp: stat.Hp,
                atk: stat.Atk,
                def: stat.Def,
                spd: stat.Spd,
                range: range,
                attackIntervalMs: DefaultMonsterAttackIntervalMs,
                critRate: stat.CritRate,
                critDamage: stat.CritDamage
            );
        }

        // CharacterDetailPb → CombatActorDef
        private async Task<CombatActorDef> MapCharacterToCombatActorDefAsync(
            CharacterDetailPb ch,
            int level,
            int tier,
            CancellationToken ct)
        {
            var baseStat = ch.StatProgressions
                .OrderBy(s => s.Level)
                .LastOrDefault(s => s.Level <= level)
                       ?? ch.StatProgressions.OrderBy(s => s.Level).First();

            var promo = ch.Promotions
                .OrderBy(p => p.Tier)
                .LastOrDefault(p => p.Tier <= tier);

            int bonusHp = 0;
            int bonusAtk = 0;
            int bonusDef = 0;
            int bonusSpd = 0;

            double bonusCritRate = 0.0;
            double bonusCritDamage = 0.0;

            if (promo != null && promo.Bonus != null)
            {
                if (promo.Bonus.Hp != null)
                    bonusHp = promo.Bonus.Hp.Value; 
                if (promo.Bonus.Atk != null)
                    bonusAtk = promo.Bonus.Atk.Value;
                if (promo.Bonus.Def != null)
                    bonusDef = promo.Bonus.Def.Value;
                if (promo.Bonus.Spd != null)
                    bonusSpd = promo.Bonus.Spd.Value;
                if (promo.Bonus.CritRate != null)
                    bonusCritRate = promo.Bonus.CritRate.Value;
                if (promo.Bonus.CritDamage != null)
                    bonusCritDamage = promo.Bonus.CritDamage.Value;
            }
            
            int hp = baseStat.Hp + bonusHp;
            int atk = baseStat.Atk + bonusAtk;
            int def = baseStat.Def + bonusDef;
            int spd = baseStat.Spd + bonusSpd;

            double critRate = baseStat.CritRate + bonusCritRate;
            double critDamage = baseStat.CritDamage + bonusCritDamage;

            var range = await _rangeReader.GetRangeAsync(ch.Id, isPlayer: true, ct)
                        ?? GuessRangeForCharacter(ch);

            return new CombatActorDef(
                masterId: ch.Id,
                isPlayer: true,
                modelKey: ch.IconUrl ?? ch.PortraitUrl ?? "", // TODO: 실제 모델키 필드가 있으면 그걸로
                maxHp: hp,
                atk: atk,
                def: def,
                spd: spd,
                range: range,
                attackIntervalMs: DefaultPlayerAttackIntervalMs,
                critRate: critRate,
                critDamage: critDamage
            );
        }

        // 없을 때 임시로 근/원거리 판별하는 규칙 (원하면 나중에 치환)
        private static float GuessRangeForMonster(MonsterPb monster)
        {
            // TODO: monster.ai_profile 이나 meta에 따라 근/원거리 구분
            // 지금은 일단 전부 근거리
            return DefaultMeleeRange;
        }

        private static float GuessRangeForCharacter(CharacterDetailPb ch)
        {
            // TODO: role_id, meta(tags)에 따라 분리 (탱커/근딜/원딜 등)
            return DefaultMeleeRange;
        }
    }
}
