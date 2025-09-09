using Domain.Enum;

namespace AdminTool.Models
{
    public sealed class ModifierRowVm
    {
        public string StatCode { get; set; } = ""; // StatTypes.Code
        public StatOp Op { get; set; }            // ← Enum 바인딩
        public decimal Value { get; set; }
    }
    public sealed class StatTypePickItem
    {
        public string Code { get; set; } = "";   // 예: "ATK"
        public string Name { get; set; } = "";   // 예: "공격력"
        public bool IsPercent { get; set; }
    }
}
