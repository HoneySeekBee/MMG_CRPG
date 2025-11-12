using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AdminTool.Models
{
    public class MonsterListItemVm
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string ModelKey { get; set; } = null!;
        public int? ElementId { get; set; }
        public int? PortraitId { get; set; } 
        public int StatCount { get; set; }
        public string? PortraitUrl { get; set; }
    }
    public class MonsterEditVm
    {
        public int Id { get; set; }  // Create 때는 0

        [Required]
        [Display(Name = "이름")]
        public string Name { get; set; } = null!;

        [Required]
        [Display(Name = "프리팹/모델 키")]
        public string ModelKey { get; set; } = null!;

        [Display(Name = "속성")]
        public int? ElementId { get; set; }

        [Display(Name = "초상화")]
        public int? PortraitId { get; set; }
        public List<PortraitPickItem> PortraitChoices { get; set; } = new();
        public string? SelectedPortraitUrl { get; set; }
        public IEnumerable<SelectListItem> Elements { get; set; } = Array.Empty<SelectListItem>();
    }
    public class MonsterStatVm
    {
        public int MonsterId { get; set; }

        [Required]
        [Display(Name = "단계 / 레벨")]
        public int Level { get; set; }

        [Required]
        [Display(Name = "HP")]
        public int HP { get; set; }

        [Required]
        [Display(Name = "ATK")]
        public int ATK { get; set; }

        [Required]
        [Display(Name = "DEF")]
        public int DEF { get; set; }

        [Required]
        [Display(Name = "SPD")]
        public int SPD { get; set; }

        [Display(Name = "치명타율(%)")]
        public decimal CritRate { get; set; } = 5.00m;

        [Display(Name = "치명타피해(%)")]
        public decimal CritDamage { get; set; } = 150.00m;
    }
    public class MonsterIndexVm
    {
        public List<MonsterListItemVm> Monsters { get; set; } = new();

        public string? Search { get; set; }
        public int Page { get; set; }
        public int TotalCount { get; set; }
    }
    public class MonsterDetailVm
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string ModelKey { get; set; } = null!;
        public int? ElementId { get; set; }
        public string? PortraitUrl { get; set; }

        public List<MonsterStatVm> Stats { get; set; } = new();
    }
    public class MonsterStatsBulkVm
    { 
        public string RawTable { get; set; } = ""; 
    }
}
