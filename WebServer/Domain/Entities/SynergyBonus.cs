using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class SynergyBonus
    {
        public int SynergyId { get; private set; }
        public short Threshold { get; private set; }
        public JsonDocument Effect { get; private set; } = null!;
        public string? Note { get; private set; }
        public Synergy? Synergy { get; private set; }

        private SynergyBonus() { } // EF
        public SynergyBonus(short threshold, JsonDocument effect, string? note = null)
        {
            Threshold = threshold;
            Effect = effect ?? throw new ArgumentNullException(nameof(effect));
            Note = note;
        }
    }

}
