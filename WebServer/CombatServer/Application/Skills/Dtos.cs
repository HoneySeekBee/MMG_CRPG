
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
        public SkillEffect Effect { get; set; } = new();
    }
}
