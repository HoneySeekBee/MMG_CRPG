using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Contents
{
    public class Chapter
    {
        public int ChapterId { get; private set; }
        public int BattleId { get; private set; }
        public int ChapterNum { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        protected Chapter() { }

        public Chapter(int battleId, int chapterNum, string name, string description, bool isActive)
        {
            BattleId = battleId;
            ChapterNum = chapterNum;
            Name = name;
            Description = description;
            IsActive = isActive;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(int battleId, int chapterNum, string name, string description, bool isActive)
        {
            BattleId = battleId;
            ChapterNum = chapterNum;
            Name = name;
            Description = description;
            IsActive = isActive;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate() => IsActive = true;
        public void Deactivate() => IsActive = false;
    }
}
