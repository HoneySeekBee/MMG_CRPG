using Application.Repositories;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Character
{
    public sealed class CharacterService : ICharacterService
    {
        private readonly ICharacterRepository _repo;

        public CharacterService(ICharacterRepository repo)
        {
            _repo = repo;
        }

        public async Task<PagedResult<CharacterSummaryDto>> GetListAsync(CharacterListQuery query, CancellationToken ct)
        {
            if (query.Page <= 0) throw new ArgumentOutOfRangeException(nameof(query.Page));
            if (query.PageSize <= 0) throw new ArgumentOutOfRangeException(nameof(query.PageSize));

            var (items, total) = await _repo.GetPagedAsync(query.Page, query.PageSize, query.ElementId, query.RarityId, query.Search, ct);
            var dtos = items.Select(x => x.ToSummaryDto()).ToList();
            return new PagedResult<CharacterSummaryDto>(dtos, total, query.Page, query.PageSize);
        }

        public async Task<CharacterDetailDto?> GetDetailAsync(int characterId, CancellationToken ct)
        {
            var (c, skills, progs, promos) = await _repo.GetAggregateAsync(characterId, ct);
            if (c is null) return null;
            return c.ToDetailDto(skills, progs, promos);
        }

        public async Task<int> CreateAsync(CreateCharacterRequest req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.Name)) throw new ArgumentException("Name required");

            var entity = Domain.Entities.Character.Create(
                name: req.Name.Trim(),
                rarityId: req.RarityId,
                factionId: req.FactionId,
                roleId: req.RoleId,
                elementId: req.ElementId,
                iconId: req.IconId,
                portraitId: req.PortraitId,
                releaseDate: req.ReleaseDate,
                isLimited: req.IsLimited,
                tags: req.Tags,
                metaJson: req.MetaJson
            );

            await _repo.AddAsync(entity, ct);
            await _repo.SaveChangesAsync(ct);
            return entity.Id;
        }

        public async Task UpdateBasicAsync(int characterId, UpdateCharacterRequest req, CancellationToken ct)
        {
            var entity = await _repo.GetByIdAsync(characterId, ct)
                         ?? throw new InvalidOperationException("Character not found");

            entity.Rename(req.Name);
            // 단순 세터 사용
            if (entity.RarityId != req.RarityId) typeof(Domain.Entities.Character).GetProperty("RarityId")!.SetValue(entity, req.RarityId);
            if (entity.FactionId != req.FactionId) typeof(Domain.Entities.Character).GetProperty("FactionId")!.SetValue(entity, req.FactionId);
            if (entity.RoleId != req.RoleId) typeof(Domain.Entities.Character).GetProperty("RoleId")!.SetValue(entity, req.RoleId);
            if (entity.ElementId != req.ElementId) typeof(Domain.Entities.Character).GetProperty("ElementId")!.SetValue(entity, req.ElementId);

            entity.SetIcon(req.IconId);
            entity.SetPortrait(req.PortraitId);
            entity.SetReleaseDate(req.ReleaseDate);
            entity.SetLimited(req.IsLimited);
            entity.SetMeta(req.MetaJson);

            // 태그 전체 교체
            var tagField = typeof(Domain.Entities.Character).GetField("_tags", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (tagField != null)
            {
                var list = (List<string>)tagField.GetValue(entity)!;
                list.Clear();
                if (req.Tags != null)
                {
                    foreach (var t in req.Tags)
                    {
                        var tag = (t ?? string.Empty).Trim();
                        if (tag.Length > 0 && !list.Contains(tag)) list.Add(tag);
                    }
                }
            }

            await _repo.SaveChangesAsync(ct);
        }

        public async Task SetSkillsAsync(int characterId, IReadOnlyList<UpsertSkillRequest> skills, CancellationToken ct)
        {
            // 슬롯 중복 방지
            if (skills.GroupBy(s => s.Slot).Any(g => g.Count() > 1))
                throw new ArgumentException("Duplicate slots");

            var domain = skills.Select(s =>
                CharacterSkill.Create(characterId, s.Slot, s.SkillId, s.UnlockTier, s.UnlockLevel)).ToList();

            await _repo.ReplaceSkillsAsync(characterId, domain, ct);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task SetProgressionsAsync(int characterId, IReadOnlyList<UpsertProgressionRequest> progressions, CancellationToken ct)
        {
            if (progressions.GroupBy(p => p.Level).Any(g => g.Count() > 1))
                throw new ArgumentException("Duplicate levels");

            var domain = progressions.Select(p =>
                CharacterStatProgression.Create(characterId, p.Level, p.HP, p.ATK, p.DEF, p.SPD, p.CritRate, p.CritDamage)).ToList();

            await _repo.ReplaceProgressionsAsync(characterId, domain, ct);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task SetPromotionsAsync(int characterId, IReadOnlyList<UpsertPromotionRequest> promotions, CancellationToken ct)
        {
            if (promotions.GroupBy(p => p.Tier).Any(g => g.Count() > 1))
                throw new ArgumentException("Duplicate tiers");

            var domain = promotions.Select(p =>
            {
                var bonus = p.Bonus is null ? null
                    : new StatModifier(p.Bonus.HP, p.Bonus.ATK, p.Bonus.DEF, p.Bonus.SPD, p.Bonus.CritRate, p.Bonus.CritDamage);

                var mats = (p.Materials ?? Array.Empty<PromotionMaterialRequest>())
                           .Select(m => new PromotionMaterial(m.ItemId, m.Quantity));

                return CharacterPromotion.Create(characterId, p.Tier, p.MaxLevel, p.CostGold, bonus, mats);
            }).ToList();

            await _repo.ReplacePromotionsAsync(characterId, domain, ct);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int characterId, CancellationToken ct)
        {
            var entity = await _repo.GetByIdAsync(characterId, ct)
                         ?? throw new InvalidOperationException("Character not found");
            await _repo.RemoveAsync(entity, ct);
            await _repo.SaveChangesAsync(ct);
        }
    }
}
