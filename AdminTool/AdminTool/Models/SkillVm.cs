using Application.SkillLevels;
using Application.Skills;
using Domain.Enum;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AdminTool.Models
{
    public sealed class SkillListItemVm
    {
        // [1] 기본 정보 
        public int SkillId { get; init; }
        public string Name { get; init; } = "";
        public int IconId { get; init; }
        public string? IconUrl { get; init; }

        // [2] 전투 정보
        public SkillType Type { get; init; }
        public int ElementId { get; init; }
        public SkillTargetingType TargetingType { get; init; }
        public TargetSideType TargetSide { get; init; }
        public AoeShapeType AoeShape { get; init; }

        // [3] 기타 정보 
        public string[] Tag { get; init; } = Array.Empty<string>();
        public bool IsActive { get; init; }

        public static SkillListItemVm From(SkillListItemDto dto, string? iconUrl) => new()
        {
            SkillId = dto.SkillId,
            Name = dto.Name,
            Type = dto.Type,
            ElementId = dto.ElementId,
            IconId = dto.IconId,
            IconUrl = iconUrl,
            IsActive = dto.IsActive,
            TargetingType = dto.TargetingType,
            TargetSide = dto.TargetSide,
            AoeShape = dto.AoeShape,
            Tag = dto.Tag ?? Array.Empty<string>()
        };
    }
    public sealed record class SkillIndexVm
    {
        // 필터
        public SkillType? Type { get; init; }
        public int? ElementId { get; init; }
        public string? NameContains { get; init; }

        // 확장 필터 
        public bool? IsActive { get; init; }
        public SkillTargetingType? TargetingType { get; init; }
        public TargetSideType? TargetSide { get; init; }
        public AoeShapeType? AoeShape { get; init; }
        public string[]? TagsAny { get; init; }      // “하나라도 포함”
        public string[]? TagsAll { get; init; }      // “모두 포함”

        // 결과
        public IReadOnlyList<SkillListItemVm> Items { get; init; } = new List<SkillListItemVm>();

        // UI 선택값
        public IReadOnlyList<SelectListItem> TypeOptions { get; init; } = new List<SelectListItem>();
        public IReadOnlyList<SelectListItem> ElementOptions { get; init; } = new List<SelectListItem>();

        // 확장 
        public IReadOnlyList<SelectListItem> TargetingTypeOptions { get; init; } = new List<SelectListItem>();
        public IReadOnlyList<SelectListItem> TargetSideOptions { get; init; } = new List<SelectListItem>();
        public IReadOnlyList<SelectListItem> AoeShapeOptions { get; init; } = new List<SelectListItem>();

        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 50;
        public int TotalCount { get; init; }
        public string SortBy { get; init; } = "name";
        public bool Desc { get; init; }
    }

    public sealed record class SkillCreateVm
    {
        // [1] 기본 정보
        [Required, StringLength(100)]
        public string Name { get; set; } = "";
        public int IconId { get; set; }
        public string? IconUrl { get; set; }     // 선택 프리뷰용
        // [2] 전투 정보
        [Required]
        public SkillType Type { get; set; } = SkillType.Unknown;
        [Required]
        public int ElementId { get; set; }
        public SkillTargetingType TargetingType { get; set; } = SkillTargetingType.None;
        public AoeShapeType AoeShape { get; set; } = AoeShapeType.None;
        public TargetSideType TargetSide { get; set; } = TargetSideType.None;
        // [3] 기타 정보
        public bool? IsActive { get; init; }
        public string[] Tag { get; set; } = Array.Empty<string>();
        public string TagInput { get; set; } = "";
        public string? Etc { get; set; }
        public string? BaseInfo { get; set; }   // JSON 에디터 바인딩 (예: Monaco)


        public List<IconPickItem> Icons { get; set; } = new(); // 아이콘 선택용 리스트

        // 드롭다운
        public IReadOnlyList<SelectListItem> TypeOptions { get; set; } = new List<SelectListItem>();
        public IReadOnlyList<SelectListItem> ElementOptions { get; set; } = new List<SelectListItem>();
        public IReadOnlyList<SelectListItem> TargetingTypeOptions { get; set; } = new List<SelectListItem>();
        public IReadOnlyList<SelectListItem> TargetSideOptions { get; set; } = new List<SelectListItem>();
        public IReadOnlyList<SelectListItem> AoeShapeOptions { get; set; } = new List<SelectListItem>();

    }
    public sealed record class SkillEditVm
    {
        // [1] 기본 정보 
        public int SkillId { get; init; }

        [Required, StringLength(100)]
        public string Name { get; set; } = "";
        public int IconId { get; set; }
        public string? IconUrl { get; set; }     // 프리뷰

        // [2] 전투 정보 
        [Required]
        public SkillType Type { get; set; }
        [Required]
        public int ElementId { get; set; }
        public SkillTargetingType TargetingType { get; set; }
        public TargetSideType TargetSide { get; set; }
        public AoeShapeType AoeShape { get; set; }

        // [3] 기타 정보 
        public bool IsActive { get; set; }
        public string[] Tag { get; set; } = Array.Empty<string>();
        public string? BaseInfo { get; set; }
        public string? Etc { get; set; }

        // 드롭다운
        public IReadOnlyList<SelectListItem> TypeOptions { get; init; } = new List<SelectListItem>();
        public IReadOnlyList<SelectListItem> ElementOptions { get; init; } = new List<SelectListItem>();
        public IReadOnlyList<SelectListItem> TargetingTypeOptions { get; init; } = new List<SelectListItem>();
        public IReadOnlyList<SelectListItem> TargetSideOptions { get; init; } = new List<SelectListItem>();
        public IReadOnlyList<SelectListItem> AoeShapeOptions { get; init; } = new List<SelectListItem>();


        // 아이콘 선택 리스트
        public List<IconPickItem> Icons { get; set; } = new();
        public static SkillEditVm From(
            SkillDto s,
            IReadOnlyList<SelectListItem> typeOptions,
            IReadOnlyList<SelectListItem> elementOptions,
            IReadOnlyList<SelectListItem> targetingTypeOptions,
            IReadOnlyList<SelectListItem> targetSideOptions,
            IReadOnlyList<SelectListItem> aoeShapeOptions,
            string? iconUrl = null,
            List<IconPickItem>? icons = null)
            => new()
            {
                SkillId = s.SkillId,
                Name = s.Name,
                Type = s.Type,
                ElementId = s.ElementId,
                IconId = s.IconId,
                IconUrl = iconUrl,

                IsActive = s.IsActive,
                TargetingType = s.TargetingType,
                TargetSide = s.TargetSide,
                AoeShape = s.AoeShape,
                Tag = s.Tag ?? Array.Empty<string>(),
                BaseInfo = s.BaseInfo?.ToJsonString(),

                TypeOptions = typeOptions,
                ElementOptions = elementOptions,
                TargetingTypeOptions = targetingTypeOptions,
                TargetSideOptions = targetSideOptions,
                AoeShapeOptions = aoeShapeOptions,
                Icons = icons ?? new()
            };
    }
    public sealed class SkillLevelsVm
    {
        public int SkillId { get; init; }
        public IReadOnlyList<SkillLevelDto> Items { get; init; } = new List<SkillLevelDto>();
    }
    public sealed class SkillLevelFormVm
    {
        public int SkillId { get; init; }
        public int Level { get; set; } // Create 시 수정가능, Update 시 read-only 처리

        public string? ValuesJson { get; set; }      // {"scale":2.0,"burn":{"dmg":20,"dur":5}}
        public string? Description { get; set; }
        public string? MaterialsJson { get; set; }   // {"501":3,"777":1}
        public int CostGold { get; set; }
    }
}
