using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Character
{
    // 목록 조회 필터/페이징
    public sealed record CharacterListQuery(
        int Page = 1,
        int PageSize = 20,
        int? ElementId = null,
        int? RarityId = null,
        string? Search = null);

    // 생성
    public sealed record CreateCharacterRequest(
        string Name,
        int RarityId,
        int FactionId,
        int RoleId,
        int ElementId,
        int? IconId = null,
        int? PortraitId = null,
        DateTimeOffset? ReleaseDate = null,
        bool IsLimited = false,
        IReadOnlyList<string>? Tags = null,
        string? MetaJson = null);

    // 기본 정보 수정
    public sealed record UpdateCharacterRequest(
        string Name,
        int RarityId,
        int FactionId,
        int RoleId,
        int ElementId,
        int? IconId,
        int? PortraitId,
        DateTimeOffset? ReleaseDate,
        bool IsLimited,
        IReadOnlyList<string>? Tags,
        string? MetaJson);

    // 스킬 세트 업서트(슬롯 기준)
    public sealed record UpsertSkillRequest(
        SkillSlot Slot, int SkillId, short UnlockTier = 0, short UnlockLevel = 1);

    // 레벨 스탯 업서트
    public sealed record UpsertProgressionRequest(
        short Level, int HP, int ATK, int DEF, int SPD, decimal CritRate = 5, decimal CritDamage = 150);

    // 승급 업서트
    public sealed record UpsertPromotionRequest(
        short Tier, short MaxLevel, int CostGold,
        StatModifierRequest? Bonus,
        IReadOnlyList<PromotionMaterialRequest>? Materials);

    public sealed record PromotionMaterialRequest(int ItemId, int Quantity);
    public sealed record StatModifierRequest(
        int? HP = null, int? ATK = null, int? DEF = null, int? SPD = null,
        decimal? CritRate = null, decimal? CritDamage = null);
}

