using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Synergy
    {
        public int SynergyId { get; private set; }
        public string Key { get; private set; } = null!;
        public string Name { get; private set; } = null!;
        public string Description { get; private set; } = null!;
        public int? IconId { get; private set; }
        public JsonDocument Effect { get; private set; } = null!;
        public Stacking Stacking { get; private set; } = Stacking.None;
        public bool IsActive { get; private set; } = true;
        public DateTime? StartAt { get; private set; }
        public DateTime? EndAt { get; private set; }

        private readonly List<SynergyBonus> _bonuses = new();
        private readonly List<SynergyRule> _rules = new();

        public IReadOnlyCollection<SynergyBonus> Bonuses => _bonuses;
        public IReadOnlyCollection<SynergyRule> Rules => _rules;

        private Synergy() { } // EF

        public Synergy(
            string key, string name, string description,
            JsonDocument effect, Stacking stacking,
            int? iconId = null, bool isActive = true,
            DateTime? startAt = null, DateTime? endAt = null)
        {
            Key = key;
            Name = name;
            Description = description;
            Effect = effect ?? throw new ArgumentNullException(nameof(effect));
            Stacking = stacking;
            IconId = iconId;
            IsActive = isActive;
            StartAt = startAt;
            EndAt = endAt;
        }

        // 서비스에서 자식 추가할 공개 메서드
        public void AddBonus(SynergyBonus bonus) => _bonuses.Add(bonus);
        public void AddRule(SynergyRule rule) => _rules.Add(rule);
    }
}
