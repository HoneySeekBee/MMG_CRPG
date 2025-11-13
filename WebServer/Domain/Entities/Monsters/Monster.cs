using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Monsters
{
    public class Monster
    {
        // PK
        public int Id { get; private set; }

        // 기본 정보
        public string Name { get; private set; } = null!;
        public string ModelKey { get; private set; } = null!;
        public int? ElementId { get; private set; }
        public int? PortraitId { get; private set; }

        // 단계별 스탯
        private readonly List<MonsterStatProgression> _stats = new();
        public IReadOnlyCollection<MonsterStatProgression> Stats => _stats;

        // EF용 빈 생성자
        private Monster() { }

        public Monster(string name, string modelKey, int? elementId = null, int? portraitId = null)
        {
            Name = name;
            ModelKey = modelKey;
            ElementId = elementId;
            PortraitId = portraitId;
        }

        public void AddOrUpdateStat(
            int level,
            int hp,
            int atk,
            int def,
            int spd,
            decimal critRate,
            decimal critDamage,
            float range)
        {
            var existing = _stats.FirstOrDefault(s => s.Level == level);
            if (existing is null)
            {
                _stats.Add(new MonsterStatProgression(
                    level,
                    hp,
                    atk,
                    def,
                    spd,
                    critRate,
                    critDamage,
                    range));
            }
            else
            {
                existing.Update(hp, atk, def, spd, critRate, critDamage, range);
            }
        }
        public void Update(string name, string modelKey, int? elementId, int? portraitId)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Monster name cannot be empty.", nameof(name));

            if (string.IsNullOrWhiteSpace(modelKey))
                throw new ArgumentException("ModelKey cannot be empty.", nameof(modelKey));

            Name = name;
            ModelKey = modelKey;
            ElementId = elementId;
            PortraitId = portraitId;
        }
    }
}
