 

namespace Infrastructure.Persistence
{
    public sealed class CombatRecord
    {
        public long Id { get; set; }
        public string Mode { get; set; } = default!;           // 'pve' | 'pvp'
        public long? StageId { get; set; }
        public long Seed { get; set; }
        public string InputJson { get; set; } = default!;      // jsonb
        public string? Result { get; set; }                    // 'win' | 'lose' | 'error' | null
        public int? ClearMs { get; set; }
        public string? BalanceVersion { get; set; }
        public string? ClientVersion { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public ICollection<CombatLogRecord> Logs { get; set; } = new List<CombatLogRecord>();
    }

    public sealed class CombatLogRecord
    {
        public long Id { get; set; }
        public long CombatId { get; set; }
        public int TMs { get; set; }                           // t_ms
        public string PayloadJson { get; set; } = default!;    // jsonb

        public CombatRecord Combat { get; set; } = default!;
    }
}
