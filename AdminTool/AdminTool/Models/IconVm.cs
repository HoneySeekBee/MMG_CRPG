using System.ComponentModel.DataAnnotations;

namespace AdminTool.Models
{
    public class IconVm
    {
        public int IconId { get; set; }
        public string Key { get; set; } = "";
        public int Version { get; set; }
        public string Url { get; set; } = "";
    }
    public class IconCreateVm
    {
        [Required(ErrorMessage = "Key는 필수입니다.")]
        [StringLength(64, ErrorMessage = "Key는 최대 64자입니다.")]
        [RegularExpression(@"^[a-zA-Z0-9._-]+$",
            ErrorMessage = "영문/숫자/.-_만 사용할 수 있어요.")]
        public string Key { get; set; } = "";


        [Required(ErrorMessage = "아이콘 이미지를 선택하세요.")]
        public IFormFile? IconFile { get; set; }
    }
    public class IconEditVm
    {
        [Required]
        public int IconId{ get; set; }

        [Required]
        [StringLength(64)]
        public string Key { get; set; } = "";
        public int CurrentVersion { get; set; }

        // 미리 보기용
        public string ImageUrl { get; set; } = "";
        [Display(Name = "새 이미지 (선택)")]
        public IFormFile? IconFile { get; set; }
    }
}
