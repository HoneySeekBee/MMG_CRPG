using Domain.Entities.Characters;
using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Character
{
    public sealed record CharacterSummaryDto(
        int Id, string Name, int RarityId, int ElementId, int RoleId, int FactionId,
        bool IsLimited, DateTimeOffset? ReleaseDate);

    public sealed record CharacterSkillDto(
        SkillSlot Slot, int SkillId, short UnlockTier, short UnlockLevel);

    public sealed record CharacterStatProgressionDto(
        short Level, int HP, int ATK, int DEF, int SPD, decimal CriRate, decimal CriDamage, float Range);

    public sealed record PromotionMaterialDto(int ItemId, int Quantity);

    public sealed record StatModifierDto(
        int? HP, int? ATK, int? DEF, int? SPD, decimal? CritRate, decimal? CritDamage);

    public sealed record CharacterPromotionDto(
        int Tier, short MaxLevel, int CostGold, StatModifierDto? Bonus, IReadOnlyList<PromotionMaterialDto> Materials);

    public sealed record CharacterDetailDto(
        int Id, string Name,
        int RarityId, int ElementId, int RoleId, int FactionId,
        int? IconId, int? PortraitId, bool IsLimited, DateTimeOffset? ReleaseDate, short formationNum,
        IReadOnlyList<string> Tags, string? MetaJson,
        IReadOnlyList<CharacterSkillDto> Skills,
        IReadOnlyList<CharacterStatProgressionDto> StatProgressions,
        IReadOnlyList<CharacterPromotionDto> Promotions);

    // 공통 페이지 결과
    public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);

    // ===== Mapping =====
    public static class CharacterMappings
    {
        public static CharacterSummaryDto ToSummaryDto(this Domain.Entities.Characters.Character c) =>
            new(c.Id, c.Name, c.RarityId, c.ElementId, c.RoleId, c.FactionId, c.IsLimited, c.ReleaseDate);

        public static CharacterDetailDto ToDetailDto(
            this Domain.Entities.Characters.Character c,
            IEnumerable<CharacterSkill> skills,
            IEnumerable<CharacterStatProgression> progressions,
            IEnumerable<CharacterPromotion> promotions)
        {
            var skillDtos = skills.Select(s =>
                new CharacterSkillDto(s.Slot, s.SkillId, s.UnlockTier, s.UnlockLevel)).ToList();

            var progDtos = progressions.Select(p =>
                new CharacterStatProgressionDto(p.Level, p.HP, p.ATK, p.DEF, p.SPD, p.CriRate, p.CriDamage, p.Range)).ToList();

            var promoDtos = promotions.Select(p =>
                new CharacterPromotionDto(
                    p.Tier, p.MaxLevel, p.CostGold,
                    p.Bonus is null ? null : new StatModifierDto(p.Bonus.HP, p.Bonus.ATK, p.Bonus.DEF, p.Bonus.SPD, p.Bonus.CritRate, p.Bonus.CritDamage),
                    p.Materials.Select(m => new PromotionMaterialDto((int)m.ItemId, m.Count)).ToList()
                )).ToList();

            return new CharacterDetailDto(
                c.Id, c.Name, c.RarityId, c.ElementId, c.RoleId, c.FactionId,
                c.IconId, c.PortraitId, c.IsLimited, c.ReleaseDate, c.FormationNumber,
                c.Tags.ToList(), c.MetaJson, skillDtos, progDtos, promoDtos);
        }
    }
}