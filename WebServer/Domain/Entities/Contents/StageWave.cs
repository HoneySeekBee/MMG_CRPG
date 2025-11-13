using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Contents
{
    public sealed class StageWave
    {
        public int Id { get; private set; }
        public int StageId { get; private set; }
        public short Index { get; private set; } // 1..N
        public int BatchNum { get; private set; }
        public List<StageWaveEnemy> Enemies { get; private set; } = new();

        public StageWave(short index) => Index = index;

        public void Validate()
        {
            if (Index < 1) throw new DomainException("INVALID_WAVE_INDEX", "Wave index must be ≥ 1");
            if (Enemies.Count == 0) throw new DomainException("INVALID_WAVE_ENEMIES", "Wave must have at least one enemy.");
            foreach (var e in Enemies) e.Validate();
            var duplicateSlot = Enemies.GroupBy(e => e.Slot).FirstOrDefault(g => g.Count() > 1);
            if (duplicateSlot != null)
                throw new DomainException("DUPLICATE_SLOT", $"Slot {duplicateSlot.Key} appears more than once.");
        }
    }

}
