namespace Infrastructure.Persistence
{
    // Read-only models mapping to WebServer-owned master data tables

    public sealed class CharacterRow
    {
        public long Id { get; set; }
    }

    public sealed class CharacterStatRow
    {
        public long CharacterId { get; set; }
        public short Level { get; set; }
        public int HP { get; set; }
        public int ATK { get; set; }
        public int DEF { get; set; }
        public int SPD { get; set; }
        public decimal CriRate { get; set; }
        public decimal CriDamage { get; set; }
        public float Range { get; set; }
    }

    public sealed class SkillRow
    {
        public int SkillId { get; set; }
        public short Type { get; set; }
        public short TargetingType { get; set; }
        public short AoeShape { get; set; }
        public short TargetSide { get; set; }
        public string? BaseInfo { get; set; }
    }

    public sealed class UserCharacterRow
    {
        public int UserCharacterId { get; set; }
        public int UserId { get; set; }
        public int CharacterId { get; set; }
        public short Level { get; set; }
    }

    public sealed class MonsterStatRow
    {
        public int MonsterId { get; set; }
        public int Level { get; set; }
        public int HP { get; set; }
        public int ATK { get; set; }
        public int DEF { get; set; }
        public int SPD { get; set; }
        public decimal CritRate { get; set; }
        public decimal CritDamage { get; set; }
        public float Range { get; set; }
    }
}
