using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Domain.Entities.Skill
{
    public sealed class SkillLevel
    {
        public int SkillId { get; private set; }
        public int Level { get; private set; }

        public IReadOnlyDictionary<string, object>? Values { get; private set; }
        public string? Description { get; private set; }
        public IReadOnlyDictionary<string, int>? Materials { get; private set; }
        public int CostGold { get; private set; }

        private SkillLevel() { }

        public SkillLevel(int skillId, int level,
            IDictionary<string, object>? values,
            string? description,
            IDictionary<string, int>? materials,
            int costGold)
        {
            if (level <= 0) throw new ArgumentOutOfRangeException(nameof(level));
            if (costGold < 0) throw new ArgumentOutOfRangeException(nameof(costGold));

            SkillId = skillId;
            Level = level;
            Values = values is null ? null : new Dictionary<string, object>(values);
            Description = description?.Trim();
            Materials = materials is null ? null : new Dictionary<string, int>(materials);
            CostGold = costGold;
        }

        public void Update(
            IDictionary<string, object>? values,
            string? description,
            IDictionary<string, int>? materials,
            int costGold)
        {
            if (costGold < 0) throw new ArgumentOutOfRangeException(nameof(costGold));

            Values = values is null ? null : new Dictionary<string, object>(values);
            Description = description?.Trim();
            Materials = materials is null ? null : new Dictionary<string, int>(materials);
            CostGold = costGold;
        }
    }
}