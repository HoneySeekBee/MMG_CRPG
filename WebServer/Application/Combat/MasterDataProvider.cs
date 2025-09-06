using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Combat
{
    public sealed class MasterDataProvider : IMasterDataProvider
    {
        private readonly IStageReader _stage;
        private readonly ICharacterReader _char;
        private readonly ISkillReader _skill;

        // 크리 기본값 (DTO에 없으므로 임시 적용)
        private const float DefaultCritRate = 0.10f; // 10%
        private const float DefaultCritDmg = 0.50f; // +50%

        // 적 레벨 기본값 (Stage에 없으면 사용)
        private const int DefaultEnemyLevel = 1;

        public MasterDataProvider(IStageReader stage, ICharacterReader @char, ISkillReader skill)
        {
            _stage = stage; _char = @char; _skill = skill;
        }

        public async Task<Domain.Services.MasterDataPack> BuildPackAsync(
            int stageId, IReadOnlyCollection<long> partyCharacterIds, CancellationToken ct)
        {
            // 1) 스테이지 로드
            var stage = await _stage.GetAsync(stageId, ct)
                       ?? throw new KeyNotFoundException($"stage {stageId} not found");

            // 2) 등장 캐릭터 ID (아군+적)
            var enemyIds = stage.EnemyCharacterIds ?? Array.Empty<long>(); // <- 네 DTO에 맞춤
            var allCharIds = enemyIds.Concat(partyCharacterIds).Distinct().ToArray();

            // 3) 캐릭터 마스터 벌크 로드
            var chars = await LoadCharactersAsync(allCharIds, ct);

            // 4) 스킬 정의 (MVP: 비워도 동작)
            var skills = await LoadSkillsAsync(Array.Empty<long>(), ct);

            // 5) 도메인 StageDef 구성
            //    네 DTO에 EnemySpawns 가 없으므로 EnemyCharacterIds 로부터 레벨=기본값으로 생성
            var enemies = enemyIds
                .Select(cid => new Domain.Services.EnemySpawn(cid, DefaultEnemyLevel))
                .ToList();

            var stageDef = new Domain.Services.StageDef(
                StageId: stage.StageId, // long
                Enemies: enemies
            );

            return new Domain.Services.MasterDataPack(
                Stage: stageDef,
                Characters: chars,
                Skills: skills
            );
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
