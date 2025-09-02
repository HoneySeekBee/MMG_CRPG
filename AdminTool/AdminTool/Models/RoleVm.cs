using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminTool.Models
{
    public class RoleVm
    {
        public int RoleId { get; set; }
        public string Key { get; set; } = "";
        public string Label { get; set; } = "";
        public string? ColorHex { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public string? Meta { get; set; }
        public int? IconId { get; set; }
        public string? IconUrl { get; set; }
    }
    public class RoleCreateVm
    {
        [Required]
        [StringLength(50)]
        public string Key { get; set; } = "";

        [Required]
        [StringLength(100)]
        public string Label { get; set; } = "";

        [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "올바른 색상 코드(#RRGGBB) 형식이어야 합니다.")]
        public string? ColorHex { get; set; }

        public int? IconId { get; set; }
        [Range(0, 9999)]
        public short SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public string? Meta { get; set; }
        public List<IconPickItem> Icons { get; set; } = new();
    }
    public class RoleEditVm
    {
        [Required]
        public int RoleId { get; set; }

        [Required]
        [StringLength(100)]
        public string Label { get; set; } = "";

        [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "올바른 색상 코드(#RRGGBB) 형식이어야 합니다.")]
        public string? ColorHex { get; set; }
        public int? IconId { get; set; }

        [Range(0, 9999)]
        public short SortOrder { get; set; }

        public bool IsActive { get; set; }

        public string? Meta { get; set; }
        public List<IconPickItem> Icons { get; set; } = new();
    }
}
