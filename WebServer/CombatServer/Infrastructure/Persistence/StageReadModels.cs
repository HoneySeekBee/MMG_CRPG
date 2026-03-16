namespace Infrastructure.Persistence
{
    // Keyless read models — map directly to existing Stage tables (read-only)
    public sealed class StageRow
    {
        public int Id { get; set; }
        public int Chapter { get; set; }
        public short StageNum { get; set; }
        public string? Name { get; set; }
        public short RecommendedPower { get; set; }
        public short StaminaCost { get; set; }
        public bool IsActive { get; set; }
    }

    public sealed class StageWaveRow
    {
        public int Id { get; set; }
        public int StageId { get; set; }
        public short Index { get; set; }
        public int BatchNum { get; set; }
    }

    public sealed class StageWaveEnemyRow
    {
        public int Id { get; set; }
        public int StageWaveId { get; set; }
        public int EnemyCharacterId { get; set; }
        public short Level { get; set; }
        public short Slot { get; set; }
        public string? AiProfile { get; set; }
    }
}
