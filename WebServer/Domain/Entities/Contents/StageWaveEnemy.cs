using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Contents
{
    public sealed class StageWaveEnemy
    {
        public int Id { get; private set; }
        public int StageWaveId { get; private set; }
        public int EnemyCharacterId { get; private set; }
        public short Level { get; private set; }  // ≥1
        public short Slot { get; private set; }   // 1..6/9
        public string? AiProfile { get; private set; }

        public StageWaveEnemy(int enemyCharacterId, short level, short slot, string? aiProfile = null)
        {
            EnemyCharacterId = enemyCharacterId;
            Level = level;
            Slot = slot;
            AiProfile = aiProfile;
        }

        public void Validate()
        {
            if (EnemyCharacterId <= 0) throw new DomainException("INVALID_ENEMY", "EnemyCharacterId required.");
            if (Level < 1) throw new DomainException("INVALID_LEVEL", "Level must be ≥ 1");
            if (Slot < 1 || Slot > 9) throw new DomainException("INVALID_SLOT", "Slot must be between 1 and 9.");
        }
    }
}
