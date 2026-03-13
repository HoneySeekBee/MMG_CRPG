using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Domain.Entities.Skill
{
    public sealed class Skill
    {
        // [1] Basic info
        public int SkillId { get; private set; }
        public string Name { get; set; }
        public int IconId { get; set; }

        // [2] Detail info
        public int ElementId { get; set; }
        public SkillType Type { get; set; }
        public SkillTargetingType TargetingType { get; set; }
        public AoeShapeType AoeShape { get; set; }
        public TargetSideType TargetSide { get; set; }

        // [3] Misc
        public bool IsActive { get; set; }
        public JsonNode? BaseInfo { get; set; }
        public string[] Tag { get; private set; } = Array.Empty<string>();

        // [4] Skill levels
        private readonly List<SkillLevel> _levels = new();
        public IReadOnlyList<SkillLevel> Levels => _levels;

        private Skill() { }

        public Skill(int skillId, string name, SkillType type, int elementId, int iconId, SkillTargetingType targetType, AoeShapeType aoeType, TargetSideType targetSide, bool isActive = true,
            JsonNode? baseInfo = null,
            IEnumerable<string>? tags = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name is required", nameof(name));

            SkillId = skillId;
            Name = name.Trim();
            Type = type;
            ElementId = elementId;
            IconId = iconId;

            TargetingType = targetType;
            AoeShape = aoeType;
            TargetSide = targetSide;

            IsActive = isActive;
            BaseInfo = baseInfo;

            SetTags(tags);
            EnforceInvariants();
        }

        public void Rename(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name is required", nameof(name));
            Name = name.Trim();
        }

        public SkillLevel AddOrUpdateLevel(
            int level,
            JsonNode? values,
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

        public void SetTags(IEnumerable<string>? tags)
        {
            Tag = (tags ?? Enumerable.Empty<string>())
                .Select(t => t?.Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t!.ToLowerInvariant())
                .Distinct()
                .ToArray();
        }

        private void EnforceInvariants()
        {
            if (!IsActive)
            {
                if (TargetingType != SkillTargetingType.None)
                    throw new InvalidOperationException("Passive skill must have TargetingType=None.");
                if (AoeShape != AoeShapeType.None)
                    throw new InvalidOperationException("Passive skill must have AoeShape=None.");
            }
        }
    }
}
