using System.ComponentModel.DataAnnotations;

namespace AdminTool.Models
{
    public class PortraitVm
    {
        public int PortraitId { get; set; }
        public string Key { get; set; } = "";
        public int Version { get; set; }
        public string Url { get; set; } = "";
    }

    public class PortraitCreateVm
    {
        [Required(ErrorMessage = "Key는 필수입니다.")]
        [StringLength(64, ErrorMessage = "Key는 최대 64자입니다.")]
        [RegularExpression(@"^[a-zA-Z0-9._-]+$", ErrorMessage = "영문/숫자/.-_만 사용할 수 있어요.")]
        public string Key { get; set; } = "";

        [Required(ErrorMessage = "초상화 이미지를 선택하세요.")]
        public IFormFile? File { get; set; }
    }

    public class PortraitEditVm
    {
        [Required]
        public int PortraitId { get; set; }

        [Required, StringLength(64)]
        public string Key { get; set; } = "";

        public int CurrentVersion { get; set; }

        // 미리보기용
        public string ImageUrl { get; set; } = "";

        [Display(Name = "새 이미지 (선택)")]
        public IFormFile? File { get; set; }
    }
}
