using System.ComponentModel.DataAnnotations;

namespace AdminTool.Models
{
    public sealed class SynergyEditVm
    {
        public int? SynergyId { get; set; }

        [Required, MaxLength(100)]
        public string Key { get; set; } = "";

        [Required] public string Name { get; set; } = "";
        [Required] public string Description { get; set; } = "";
        public int? IconId { get; set; }
        [Required] public string EffectJson { get; set; } = """{ "stat":"ATK","op":"add_pct","value":10 }""";
        [Required] public short Stacking { get; set; } = 0;

        public bool IsActive { get; set; } = true;
        public DateTime? StartAt { get; set; }
        public DateTime? EndAt { get; set; }

        // 하위 컬렉션
        public List<BonusVm> Bonuses { get; set; } = new();
        public List<RuleVm> Rules { get; set; } = new();
        public List<TargetVm> Targets { get; set; } = new();


        // NEW: 아이콘 선택용
        public List<IconPickItem> Icons { get; set; } = new();
        public List<StatTypePickItem> StatTypes { get; set; } = new();
        public List<ModifierRowVm> EffectMods { get; set; } = new();


        public List<PickItem> Elements { get; set; } = new();
        public List<PickItem> Factions { get; set; } = new();
        public List<PickItem> ItemTags { get; set; } = new();

    }
    public sealed class PickItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    public sealed class BonusVm
    {
        [Required] public short Threshold { get; set; }
        [Required] public string EffectJson { get; set; } = """{ "stat":"ATK","op":"add_pct","value":5 }""";
        public string? Note { get; set; }
    }

    public sealed class RuleVm
    {
        [Required] public short Scope { get; set; }
        [Required] public short Metric { get; set; }
        [Required] public int RefId { get; set; }
        [Required, Range(1, int.MaxValue)] public int RequiredCnt { get; set; } = 1;
        public string? ExtraJson { get; set; }   // optional
    }

    public sealed class TargetVm
    {
        [Required] public short TargetType { get; set; }
        [Required] public int TargetId { get; set; }
    }
}
