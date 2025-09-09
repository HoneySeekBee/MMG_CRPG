namespace AdminTool.Models
{
    public sealed class StatTypeVm
    {
        public short Id { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public bool IsPercent { get; set; }
    }
}
