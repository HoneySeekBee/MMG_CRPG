using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Domain.Entities
{
    public sealed class Character
    {
        private Character() { }

        public int Id { get; private set; }

        // 필수 속성
        public string Name { get; private set; } = default!;
        public int RarityId { get; private set; }
        public int FactionId { get; private set; }
        public int RoleId { get; private set; }
        public int ElementId { get; private set; }

        // 선택 속성
        public int? IconId { get; private set; }
        public int? PortraitId { get; private set; }
        public DateTimeOffset? ReleaseDate { get; private set; }
        public bool IsLimited { get; private set; }

        // JSONB 매핑 예정: 태그 목록
        private readonly List<string> _tags = new();
        public IReadOnlyList<string> Tags => _tags;


        public ICollection<CharacterSkill> CharacterSkills { get; } = new List<CharacterSkill>();
        public ICollection<CharacterStatProgression> CharacterStatProgressions { get; } = new List<CharacterStatProgression>();
        public ICollection<CharacterPromotion> CharacterPromotions { get; } = new List<CharacterPromotion>();
        public string? MetaJson { get; private set; }
        public static Character Create(
            string name,
            int rarityId,
            int factionId,
            int roleId,
            int elementId,
            int? iconId = null,
            int? portraitId = null,
            DateTimeOffset? releaseDate = null,
            bool isLimited = false,
            IEnumerable<string>? tags = null,
            string? metaJson = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("name is required", nameof(name));

            var c = new Character
            {
                Name = name.Trim(),
                RarityId = rarityId,
                FactionId = factionId,
                RoleId = roleId,
                ElementId = elementId,
                IconId = iconId,
                PortraitId = portraitId,
                ReleaseDate = releaseDate,
                IsLimited = isLimited,
                MetaJson = metaJson
            };

            if (tags != null)
            {
                foreach (var t in tags)
                {
                    var tag = (t ?? string.Empty).Trim();
                    if (tag.Length > 0 && !c._tags.Contains(tag))
                        c._tags.Add(tag);
                }
            }
            return c;
        }
        public void Rename(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("name is required", nameof(name));
            Name = name.Trim();
        }

        public void SetLimited(bool limited) => IsLimited = limited;
        public void SetReleaseDate(DateTimeOffset? date) => ReleaseDate = date;

        public void SetIcon(int? iconId) => IconId = iconId;
        public void SetPortrait(int? portraitId) => PortraitId = portraitId;

        public void SetMeta(string? json) => MetaJson = json;

        public void AddTag(string tag)
        {
            tag = (tag ?? string.Empty).Trim();
            if (tag.Length == 0) return;
            if (!_tags.Contains(tag)) _tags.Add(tag);
        }
        public void RemoveTag(string tag) => _tags.Remove(tag);
    }
    
}
