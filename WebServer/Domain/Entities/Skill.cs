using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public sealed class Skill
    {
        public int SkillId { get; private set; }
        public string Name { get; set; }
        public SkillType Type { get; set; }
        public int ElementId { get; set; }
        public int IconId { get; set; }

        private readonly List<SkillLevel> _levels = new();
        public IReadOnlyList<SkillLevel> Levels => _levels;

        private Skill() { }

        public Skill(int skillId, string name, SkillType type, int elementId, int iconId)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name is required", nameof(name));

            SkillId = skillId;
            Name = name.Trim();
            Type = type;
            ElementId = elementId;
            IconId = iconId;
        }

        public void Rename(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name is required", nameof(name));
            Name = name.Trim();
        }

        public SkillLevel AddOrUpdateLevel(
            int level,
            IDictionary<string, object>? values,
            string? description,
            IDictionary<string, int>? materials,
            int costGold)
        {
            if (level <= 0) throw new ArgumentOutOfRangeException(nameof(level));
            if (costGold < 0) throw new ArgumentOutOfRangeException(nameof(costGold));

            var existing = _levels.FirstOrDefault(l => l.Level == level);
            if (existing is not null)
            {
                existing.Update(values, description, materials, costGold);
                return existing;
            }

            var created = new SkillLevel(SkillId, level, values, description, materials, costGold);
            _levels.Add(created);
            _levels.Sort((a, b) => a.Level.CompareTo(b.Level));
            return created;
        }
    }
}