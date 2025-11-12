using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Domain.Enum;
using Application.Character;

namespace AdminTool.Models
{
    public sealed class CharacterListFilterVm
    {
        [Range(1, int.MaxValue)] public int Page { get; set; } = 1;
        [Range(1, 200)] public int PageSize { get; set; } = 20;

        public int? ElementId { get; set; }
        public int? RarityId { get; set; }
        public int? RoleId { get; set; }
        public int? FactionId { get; set; }
        [DisplayName("검색어")] public string? Search { get; set; }

        // 드롭다운
        public IEnumerable<SelectListItem> Elements { get; set; } = Array.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Rarities { get; set; } = Array.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Roles { get; set; } = Array.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Factions { get; set; } = Array.Empty<SelectListItem>();
    }
    public sealed class CharacterSummaryVm
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public int RarityId { get; set; }
        public int ElementId { get; set; }
        public int RoleId { get; set; }
        public int FactionId { get; set; }
        public bool IsLimited { get; set; }
        public DateTimeOffset? ReleaseDate { get; set; }
    }

    public sealed class CharacterIndexVm
    {
        public CharacterListFilterVm Filter { get; set; } = new();
        public IReadOnlyList<CharacterSummaryVm> Items { get; set; } = Array.Empty<CharacterSummaryVm>();
        public int TotalCount { get; set; }
    }
    public sealed class CharacterFormVm
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        [DisplayName("이름")]
        public string Name { get; set; } = default!;

        [DisplayName("희귀도"), Range(1, int.MaxValue)] public int RarityId { get; set; }
        [DisplayName("속성"), Range(1, int.MaxValue)] public int ElementId { get; set; }
        [DisplayName("역할"), Range(1, int.MaxValue)] public int RoleId { get; set; }
        [DisplayName("진영"), Range(1, int.MaxValue)] public int FactionId { get; set; }
        public short FormationNum { get; set; }

        [DisplayName("아이콘")] public int? IconId { get; set; }
        public List<IconPickItem> IconChoices { get; set; } = new();
        [DisplayName("초상화")] public int? PortraitId { get; set; }
        public List<PortraitPickItem> PortraitChoices { get; set; } = new();
        [DisplayName("출시일")] public DateTimeOffset? ReleaseDate { get; set; }
        [DisplayName("한정")] public bool IsLimited { get; set; }

        // 태그는 폼에서 편하게 CSV로 입력받고 서버에서 분해
        [DisplayName("태그(CSV)")] public string? TagsCsv { get; set; }
        public IReadOnlyList<string> Tags =>
            string.IsNullOrWhiteSpace(TagsCsv)
                ? Array.Empty<string>()
                : TagsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                         .Distinct(StringComparer.OrdinalIgnoreCase)
                         .ToList();

        [DisplayName("메타(JSON)")]
        public string? MetaJson { get; set; }

        // 드롭다운 데이터
        public IEnumerable<SelectListItem> Elements { get; set; } = Array.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Rarities { get; set; } = Array.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Roles { get; set; } = Array.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Factions { get; set; } = Array.Empty<SelectListItem>();

        public IEnumerable<SelectListItem> Icons { get; set; } = Array.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Portraits { get; set; } = Array.Empty<SelectListItem>();
    }
    public sealed class CharacterSkillRowVm
    {
        [DisplayName("슬롯")] public SkillSlot Slot { get; set; }
        [DisplayName("스킬"), Range(1, int.MaxValue)] public int SkillId { get; set; }
        [DisplayName("해금 티어"), Range(0, 99)] public short UnlockTier { get; set; } = 0;
        [DisplayName("해금 레벨"), Range(1, 200)] public short UnlockLevel { get; set; } = 1;
    }

    public sealed class CharacterSkillsVm
    {
        public int CharacterId { get; set; }
        public List<CharacterSkillRowVm> Rows { get; set; } = new()
        {
            new() { Slot = SkillSlot.Basic },
            new() { Slot = SkillSlot.Special },
            new() { Slot = SkillSlot.Ultimate },
            new() { Slot = SkillSlot.Passive },
        };

        // 스킬 선택 드롭다운(필요 시 슬롯별 리스트도 가능)
        public IEnumerable<SelectListItem> AllSkills { get; set; } = Array.Empty<SelectListItem>();
    }
    public sealed class CharacterProgressionRowVm
    {
        [Range(1, 999)] public short Level { get; set; }

        [Range(0, int.MaxValue)] public int HP { get; set; }
        [Range(0, int.MaxValue)] public int ATK { get; set; }
        [Range(0, int.MaxValue)] public int DEF { get; set; }
        [Range(0, int.MaxValue)] public int SPD { get; set; }

        [Range(0, 100)] public decimal CritRate { get; set; } = 5m;    // %
        [Range(0, 1000)] public decimal CritDamage { get; set; } = 150m; // +%
    }
    public sealed class CharacterProgressionsVm
    {
        public int CharacterId { get; set; }
        public List<CharacterProgressionRowVm> Rows { get; set; } = new();
    }
    public sealed class StatModifierVm
    {
        public int? HP { get; set; }
        public int? ATK { get; set; }
        public int? DEF { get; set; }
        public int? SPD { get; set; }
        [Range(0, 100)] public decimal? CritRate { get; set; }
        [Range(0, 1000)] public decimal? CritDamage { get; set; }
    }

    public sealed class PromotionMaterialVm
    {
        [Range(1, int.MaxValue)] public int ItemId { get; set; }
        [Range(1, int.MaxValue)] public int Quantity { get; set; }
    }

    public sealed class CharacterPromotionRowVm
    {
        [Range(0, 99)] public short Tier { get; set; }
        [Range(1, 999)] public short MaxLevel { get; set; }
        [Range(0, int.MaxValue)] public int CostGold { get; set; }
        public StatModifierVm? Bonus { get; set; }
        public List<PromotionMaterialVm> Materials { get; set; } = new();
    }

    public sealed class CharacterPromotionsVm
    {
        public int CharacterId { get; set; }
        public List<CharacterPromotionRowVm> Rows { get; set; } = new();
        public IEnumerable<SelectListItem> Items { get; set; } = Array.Empty<SelectListItem>(); // 재료 아이템 드롭다운
    }
    public static class CharacterVmMapper
    {
        // --- 리스트/상세 DTO <-> VM ---
        public static CharacterSummaryVm FromDto(CharacterSummaryDto dto) =>
            new()
            {
                Id = dto.Id,
                Name = dto.Name,
                RarityId = dto.RarityId,
                ElementId = dto.ElementId,
                RoleId = dto.RoleId,
                FactionId = dto.FactionId,
                IsLimited = dto.IsLimited,
                ReleaseDate = dto.ReleaseDate
            };

        public static CharacterFormVm FromDetailDto(CharacterDetailDto d)
        {
            return new CharacterFormVm
            {
                Id = d.Id,
                Name = d.Name,
                RarityId = d.RarityId,
                ElementId = d.ElementId,
                RoleId = d.RoleId,
                FactionId = d.FactionId,
                IconId = d.IconId,
                PortraitId = d.PortraitId,
                IsLimited = d.IsLimited,
                ReleaseDate = d.ReleaseDate,
                TagsCsv = string.Join(", ", d.Tags),
                MetaJson = d.MetaJson
            };
        }

        // --- 폼 -> Application 요청 ---
        public static CreateCharacterRequest ToCreateRequest(this CharacterFormVm vm) =>
            new(
                vm.Name, vm.RarityId, vm.FactionId, vm.RoleId, vm.ElementId, vm.FormationNum,
                vm.IconId, vm.PortraitId, vm.ReleaseDate, vm.IsLimited,
                vm.Tags, vm.MetaJson
            );

        public static UpdateCharacterRequest ToUpdateRequest(this CharacterFormVm vm) =>
            new(
                vm.Name, vm.RarityId, vm.FactionId, vm.RoleId, vm.ElementId,
                vm.IconId, vm.PortraitId, vm.ReleaseDate, vm.IsLimited,
                vm.Tags, vm.MetaJson
            );

        public static IReadOnlyList<UpsertSkillRequest> ToSkillRequests(this CharacterSkillsVm vm) =>
            vm.Rows.Select(r => new UpsertSkillRequest(r.Slot, r.SkillId, r.UnlockTier, r.UnlockLevel)).ToList();

        public static IReadOnlyList<UpsertProgressionRequest> ToProgressionRequests(this CharacterProgressionsVm vm) =>
            vm.Rows.OrderBy(r => r.Level)
                   .Select(r => new UpsertProgressionRequest(r.Level, r.HP, r.ATK, r.DEF, r.SPD, r.CritRate, r.CritDamage))
                   .ToList();

        public static IReadOnlyList<UpsertPromotionRequest> ToPromotionRequests(this CharacterPromotionsVm vm) =>
            vm.Rows.OrderBy(r => r.Tier)
                   .Select(r => new UpsertPromotionRequest(
                       r.Tier, r.MaxLevel, r.CostGold,
                       r.Bonus is null ? null
                           : new StatModifierRequest(r.Bonus.HP, r.Bonus.ATK, r.Bonus.DEF, r.Bonus.SPD, r.Bonus.CritRate, r.Bonus.CritDamage),
                       r.Materials.Select(m => new PromotionMaterialRequest(m.ItemId, m.Quantity)).ToList()
                   ))
                   .ToList();

        // --- 상세 DTO → 하위 폼 ---
        public static CharacterSkillsVm ToSkillsVm(this CharacterDetailDto d, IEnumerable<SelectListItem>? skillList = null)
        {
            var vm = new CharacterSkillsVm
            {
                CharacterId = d.Id,
                Rows = d.Skills.Select(s => new CharacterSkillRowVm
                {
                    Slot = s.Slot,
                    SkillId = s.SkillId,
                    UnlockTier = s.UnlockTier,
                    UnlockLevel = s.UnlockLevel
                }).ToList(),
                AllSkills = skillList ?? Array.Empty<SelectListItem>()
            };

            // 슬롯 누락 시 기본 행 보충
            var need = new[] { SkillSlot.Basic, SkillSlot.Special, SkillSlot.Ultimate, SkillSlot.Passive }
                       .Except(vm.Rows.Select(r => r.Slot));
            foreach (var slot in need) vm.Rows.Add(new CharacterSkillRowVm { Slot = slot });

            return vm;
        }

        public static CharacterProgressionsVm ToProgressionsVm(this CharacterDetailDto d) =>
            new()
            {
                CharacterId = d.Id,
                Rows = d.StatProgressions
                        .OrderBy(p => p.Level)
                        .Select(p => new CharacterProgressionRowVm
                        {
                            Level = p.Level,
                            HP = p.HP,
                            ATK = p.ATK,
                            DEF = p.DEF,
                            SPD = p.SPD,
                            CritRate = p.CriRate,
                            CritDamage = p.CriDamage
                        }).ToList()
            };

        public static CharacterPromotionsVm ToPromotionsVm(this CharacterDetailDto d, IEnumerable<SelectListItem>? itemList = null) =>
            new()
            {
                CharacterId = d.Id,
                Rows = d.Promotions
                        .OrderBy(p => p.Tier)
                        .Select(p => new CharacterPromotionRowVm
                        {
                            Tier = (short)p.Tier,
                            MaxLevel = p.MaxLevel,
                            CostGold = p.CostGold,
                            Bonus = p.Bonus is null ? null : new StatModifierVm
                            {
                                HP = p.Bonus.HP,
                                ATK = p.Bonus.ATK,
                                DEF = p.Bonus.DEF,
                                SPD = p.Bonus.SPD,
                                CritRate = p.Bonus.CritRate,
                                CritDamage = p.Bonus.CritDamage
                            },
                            Materials = p.Materials.Select(m => new PromotionMaterialVm { ItemId = m.ItemId, Quantity = m.Quantity }).ToList()
                        }).ToList(),
                Items = itemList ?? Array.Empty<SelectListItem>()
            };
    }
}
