using Domain.Entities.Skill;
using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.SkillLevels
{
    public sealed class SkillLevelDto
    {
        public int SkillId { get; init; }
        public int Level { get; init; }
        public IReadOnlyDictionary<string, object>? Values { get; init; }
        public string? Description { get; init; }
        public IReadOnlyDictionary<string, int>? Materials { get; init; }
        public int CostGold { get; init; }

        public SkillType ParentType { get; set; }
        public bool IsPassive { get; set; }
        public static SkillLevelDto From(SkillLevel e) => new SkillLevelDto
        {
            SkillId = e.SkillId,
            Level = e.Level,
            Values = e.Values,
            Description = e.Description,
            Materials = e.Materials,
            CostGold = e.CostGold,
        };
    }
}
