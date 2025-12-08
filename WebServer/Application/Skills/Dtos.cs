
using Application.Combat;
using Application.SkillLevels;
using Domain.Entities.Skill;
using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Application.Skills
{
    public sealed class SkillListItemDto
    {
        // [1] 기본 정보 
        public int SkillId { get; init; }
        public string Name { get; init; } = "";
        public int IconId { get; init; }

        // [2] 상세 정보 
        public SkillType Type { get; init; }
        public int ElementId { get; init; }
        public SkillTargetingType TargetingType { get; init; }
        public TargetSideType TargetSide { get; init; }
        public AoeShapeType AoeShape { get; init; }

        // [3] 기타정보
        public bool IsActive { get; init; }
        public string[] Tag { get; init; } = Array.Empty<string>();
        public static SkillListItemDto From(Skill s) => new()
        {
            SkillId = s.SkillId,
            Name = s.Name,
            Type = s.Type,
            ElementId = s.ElementId,
            IconId = s.IconId,
            IsActive = s.IsActive,
            TargetingType = s.TargetingType,
            TargetSide = s.TargetSide,
            AoeShape = s.AoeShape,
            Tag = s.Tag ?? Array.Empty<string>()
        };
    }

    // 상세 편집할때 
    public sealed class SkillDto
    {
        // [1] 기본 정보 
        public int SkillId { get; init; }
        public string Name { get; init; } = "";
        public int IconId { get; init; }

        // [2] 상세 정보
        public SkillType Type { get; init; }
        public int ElementId { get; init; }
        public SkillTargetingType TargetingType { get; init; }
        public TargetSideType TargetSide { get; init; }
        public AoeShapeType AoeShape { get; init; }

        // [3] 기타 정보 
        public bool IsActive { get; init; }
        public string[] Tag { get; init; } = Array.Empty<string>();
        public JsonNode? BaseInfo { get; init; }
        public static SkillDto From(Skill s) => new()
        {
            SkillId = s.SkillId,
            Name = s.Name,
            Type = s.Type,
            ElementId = s.ElementId,
            IconId = s.IconId,
            IsActive = s.IsActive,
            TargetingType = s.TargetingType,
            TargetSide = s.TargetSide,
            AoeShape = s.AoeShape,
            Tag = s.Tag ?? Array.Empty<string>(),
            BaseInfo = s.BaseInfo
        };
    }
    public sealed class SkillWithLevelsDto
    {
        public int SkillId { get; init; }
        public string Name { get; init; } = "";
        public SkillType Type { get; init; }
        public int ElementId { get; init; }
        public int IconId { get; init; }

        public bool IsActive { get; init; }
        public SkillTargetingType TargetingType { get; init; }
        public TargetSideType TargetSide { get; init; }
        public AoeShapeType AoeShape { get; init; }
        public string[] Tag { get; init; } = Array.Empty<string>();
        public JsonNode? BaseInfo { get; init; }

        public IReadOnlyList<SkillLevelDto> Levels { get; init; } = new List<SkillLevelDto>();

        public SkillEffect Effect { get; init; }
        public static SkillWithLevelsDto From(Skill s) => new()
        {
            SkillId = s.SkillId,
            Name = s.Name,
            Type = s.Type,
            ElementId = s.ElementId,
            IconId = s.IconId,
            IsActive = s.IsActive,
            TargetingType = s.TargetingType,
            TargetSide = s.TargetSide,
            AoeShape = s.AoeShape,
            Tag = s.Tag ?? Array.Empty<string>(),
            BaseInfo = s.BaseInfo,
            Levels = s.Levels.Select(SkillLevelDto.From).ToList(),
            Effect = SkillEffectParser.Parse(s)
        };
    }
}
