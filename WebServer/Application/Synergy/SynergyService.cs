using Application.Repositories;
using Domain.Entities;
using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities = Domain.Entities;
using DmSynergy = Domain.Entities.Synergy;

namespace Application.Synergy
{
    public sealed class SynergyService : ISynergyService
    {
        private readonly ISynergyRepository _repo;

        public SynergyService(ISynergyRepository repo) => _repo = repo;
        public async Task<SynergyDto> CreateAsync(CreateSynergyRequest req, CancellationToken ct)
        {
            var bonusesSrc = req.Bonuses ?? Enumerable.Empty<CreateSynergyBonusRequest>();
            var rulesSrc = req.Rules ?? Enumerable.Empty<CreateSynergyRuleRequest>();

            // 생성자는 공개 생성자 사용
            var entity = new Entities.Synergy(
                key: req.Key,
                name: req.Name,
                description: req.Description,
                effect: req.Effect!,
               stacking: (Stacking)req.Stacking,
                iconId: req.IconId,
                isActive: req.IsActive,
                startAt: req.StartAt,
                endAt: req.EndAt
            );

            // 자식 추가는 메서드로
            foreach (var b in bonusesSrc)
                entity.AddBonus(new Entities.SynergyBonus(b.Threshold, b.Effect!, b.Note));

            foreach (var r in rulesSrc)
                entity.AddRule(new Entities.SynergyRule(
                    scope: (Scope)r.Scope,          // DTO가 short면 캐스팅
                    metric: (Metric)r.Metric,         // DTO가 short면 캐스팅
                    refId: r.RefId,
                    requiredCnt: r.RequiredCnt,            // 위에서 Rule ctor를 int로 맞춤
                    extra: r.Extra
                ));

            await _repo.AddAsync(entity, ct);
            var dto = new SynergyDto(
     SynergyId: entity.SynergyId,
     Key: entity.Key,
     Name: entity.Name,
     Description: entity.Description,
     IconId: entity.IconId,
     Effect: entity.Effect,
     Stacking: (short)entity.Stacking, // SynergyDto가 enum이면 캐스팅 제거
     IsActive: entity.IsActive,
     StartAt: entity.StartAt,
     EndAt: entity.EndAt,
        Bonuses: entity.Bonuses
        .Select(b => new SynergyBonusDto(
            entity.SynergyId,
            b.Threshold,
            b.Effect,
            b.Note))
        .ToList(),
       Rules: entity.Rules
        .Select(r => new SynergyRuleDto(
            entity.SynergyId,
            (short)r.Scope,   
            (short)r.Metric,  
            r.RefId,
            r.RequiredCnt,
            r.Extra))
        .ToList()
 );

            return dto;
        }
        public async Task<SynergyDto?> GetAsync(int id, CancellationToken ct)
            => (await _repo.GetAsync(id, ct)) is { } s ? Map(s) : null;

        public async Task<SynergyDto?> GetByKeyAsync(string key, CancellationToken ct)
            => (await _repo.GetByKeyAsync(key, ct)) is { } s ? Map(s) : null;

        public async Task<IReadOnlyList<SynergyDto>> GetActivesAsync(DateTime now, CancellationToken ct)
            => (await _repo.GetActiveAsync(now, ct)).Select(Map).ToList();

        public async Task<SynergyDto> UpdateAsync(UpdateSynergyRequest req, CancellationToken ct)
        {
            var s = await _repo.GetAsync(req.SynergyId, ct) ?? throw new KeyNotFoundException("Synergy not found");

            // Domain 최소형 → setter가 private이라면 Infra에서 트래킹 후 컬럼 별로 적용
            // Application에선 변경 의도를 전달:
            if (req.Name != null) s.GetType(); // no-op, 실제 값 적용은 Repository에서
                                               // 가장 간단한 방식: Repository.UpdateAsync에서 컬럼별 patch 처리

            await _repo.UpdateAsync(s, ct);
            return Map(s);
        }

        public Task DeleteAsync(int id, CancellationToken ct) => _repo.DeleteAsync(id, ct);
        public async Task<IReadOnlyList<EvaluateResult>> EvaluateAsync(
    EvaluateSynergiesRequest req, CancellationToken ct)
        {
            var actives = await _repo.GetActiveAsync(DateTime.UtcNow, ct);
            var results = new List<EvaluateResult>();

            foreach (var s in actives)
            {
                // 조건을 만족하는 “캐릭터들”
                var matched = new HashSet<int>();

                foreach (var r in s.Rules)
                {
                    switch ((Metric)r.Metric)
                    {
                        case Metric.PartyElement:
                            foreach (var c in req.Characters)
                                if (c.ElementId == r.RefId)
                                    matched.Add(c.CharacterId);
                            break;

                        case Metric.PartyFaction:
                            foreach (var c in req.Characters)
                                if (c.FactionId == r.RefId)
                                    matched.Add(c.CharacterId);
                            break;

                        case Metric.CharacterItemTag:
                            // RequiredCnt: 태그 조각(개수) 요구치로 해석
                            var need = Math.Max(1, r.RequiredCnt);
                            var tag = r.Extra?.RootElement.TryGetProperty("tag", out var p) == true ? p.GetString() : null;
                            if (!string.IsNullOrWhiteSpace(tag))
                            {
                                foreach (var c in req.Characters)
                                {
                                    var have = (c.TagCounts != null && c.TagCounts.TryGetValue(tag!, out var cnt)) ? cnt : 0;
                                    if (have >= need)
                                        matched.Add(c.CharacterId);
                                }
                            }
                            break;
                    }
                }

                // 만족한 캐릭터 수로 보너스 단계 결정
                var satisfied = matched.Count;
                short? th = s.Bonuses
                    .OrderBy(b => b.Threshold)
                    .Where(b => b.Threshold <= satisfied)
                    .Select(b => b.Threshold)
                    .LastOrDefault();

                results.Add(new EvaluateResult(s.Key, s.Name, th == 0 ? null : th));
                // 필요 시 recipients도 반환하도록 DTO 확장 가능
            }

            return results;
        }
        private static int EvaluateRuleStacks(SynergyRule r, EvaluateSynergiesRequest req)
        {
            int required = Math.Max(1, r.RequiredCnt);

            switch (r.Scope)
            {
                case Scope.Party:
                    if (r.Metric == Metric.PartyElement)
                        return (req.ElementIds?.Count(id => id == r.RefId) ?? 0) / required;

                    if (r.Metric == Metric.PartyFaction)
                        return (req.FactionIds?.Count(id => id == r.RefId) ?? 0) / required;

                    return 0;

                case Scope.Character:
                    if (r.Metric == Metric.CharacterItemTag)
                    {
                        // ExtraJson에서 tag 코드 꺼내기
                        var tag = r.Extra?.RootElement.TryGetProperty("tag", out var p) == true
                                    ? p.GetString()
                                    : null;
                        if (string.IsNullOrWhiteSpace(tag)) return 0;

                        int best = 0;
                        foreach (var ch in req.Characters ?? Array.Empty<CharacterEquipSummary>())
                        {
                            var tags = ch.TagCounts;
                            if (tags != null && tags.TryGetValue(tag, out var cnt))
                                best = Math.Max(best, cnt / required);
                        }
                        return best;
                    }
                    return 0;

                default:
                    return 0;
            }
        }

        private static SynergyDto Map(DmSynergy s) =>
            new(
                s.SynergyId, s.Key, s.Name, s.Description, s.IconId, s.Effect,
                (short)s.Stacking, s.IsActive, s.StartAt, s.EndAt,
                s.Bonuses.Select(b => new SynergyBonusDto(b.SynergyId, b.Threshold, b.Effect, b.Note)).ToList(),
                s.Rules.Select(r => new SynergyRuleDto(r.SynergyId, (short)r.Scope, (short)r.Metric, r.RefId, r.RequiredCnt, r.Extra)).ToList()
            );
    }
}
