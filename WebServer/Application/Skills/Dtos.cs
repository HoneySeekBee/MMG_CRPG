
using Domain.Entities;
using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Skills
{
    public sealed class SkillListItemDto
    {
        public int SkillId { get; init; }
        public string Name { get; init; } = "";
        public SkillType Type { get; init; }
        public int ElementId { get; init; }
        public int IconId { get; init; }

        public static SkillListItemDto From(Skill s) => new SkillListItemDto
        {
            SkillId = s.SkillId,
            Name = s.Name,
            Type = s.Type,
            ElementId = s.ElementId,
            IconId = s.IconId
        };
    }
    public sealed class SkillDto
    {
        public int SkillId { get; init; }
        public string Name { get; init; } = "";
        public SkillType Type { get; init; }
        public int ElementId { get; init; }
        public int IconId { get; init; }

        public static SkillDto From(Skill s) => new SkillDto
        {
            SkillId = s.SkillId,
            Name = s.Name,
            Type = s.Type,
            ElementId = s.ElementId,
            IconId = s.IconId
        };
    }
    public sealed class SkillLevelDto
    {
        public int SkillId { get; init; }
        public int Level { get; init; }
        public IReadOnlyDictionary<string, object>? Values { get; init; }
        public string? Description { get; init; }
        public IReadOnlyDictionary<string, int>? Materials { get; init; }
        public int CostGold { get; init; }

        public static SkillLevelDto From(SkillLevel l) => new SkillLevelDto
        {
            SkillId = l.SkillId,
            Level = l.Level,
            Values = l.Values,
            Description = l.Description,
            Materials = l.Materials,
            CostGold = l.CostGold
        };
    }
    public sealed class SkillWithLevelsDto
    {
        public int SkillId { get; init; }
        public string Name { get; init; } = "";
        public SkillType Type { get; init; }
        public int ElementId { get; init; }
        public int IconId { get; init; }
        public IReadOnlyList<SkillLevelDto> Levels { get; init; } = new List<SkillLevelDto>();

        public static SkillWithLevelsDto From(Skill s) => new SkillWithLevelsDto
        {
            SkillId = s.SkillId,
            Name = s.Name,
            Type = s.Type,
            ElementId = s.ElementId,
            IconId = s.IconId,
            Levels = s.Levels.Select(SkillLevelDto.From).ToList()
        };
    }
}
