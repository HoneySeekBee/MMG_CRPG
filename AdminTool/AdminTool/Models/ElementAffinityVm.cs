using System.ComponentModel.DataAnnotations;

namespace AdminTool.Models
{
    public class ElementAffinityVm
    {
        public int AttackerElementId { get; set; }
        public string AttackerElementLabel { get; set; } = "";
        public int DefenderElementId { get; set; }
        public string DefenderElementLabel { get; set; } = "";
        public decimal Multiplier { get; set; }
    }
    public class ElementAffinityCreateVm
    {
        [Required] public int AttackerElementId { get; set; }
        [Required] public int DefenderElementId { get; set; }

        [Range(0.00, 10.00, ErrorMessage = "0.00 ~ 10.00 사이")]
        public decimal Multiplier { get; set; } = 1.00m;

        // 드롭다운용 목록
        public List<ElementOptionVm> Elements { get; set; } = new();
    }
    public class ElementAffinityEditVm
    {
        [Required] public int AttackerElementId { get; set; }
        public string AttackerElementLabel { get; set; } = "";
        [Required] public int DefenderElementId { get; set; }
        public string DefenderElementLabel { get; set; } = "";

        [Range(0.00, 10.00, ErrorMessage = "0.00 ~ 10.00 사이")]
        public decimal Multiplier { get; set; }
    }
    public class ElementOptionVm
    {
        public int ElementId { get; set; }
        public string Key { get; set; } = "";
        public string Label { get; set; } = "";
        public override string ToString() => string.IsNullOrWhiteSpace(Label) ? Key : Label;
    }
    public class CreateElementAffinityRequest
    {
        public int AttackerElementId { get; set; }
        public int DefenderElementId { get; set; }
        public decimal Multiplier { get; set; }
    }
    public class UpdateElementAffinityRequest
    {
        public decimal Multiplier { get; set; }
    }

}
