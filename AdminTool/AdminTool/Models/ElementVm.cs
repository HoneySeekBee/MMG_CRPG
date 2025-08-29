using System.ComponentModel.DataAnnotations;

namespace AdminTool.Models
{
    public sealed class ElementVm
    {
        public int ElementId { get; set; }

        [Display(Name = "키(Key)")]
        public string Key { get; set; } = "";

        [Display(Name = "이름(Label)")]
        public string Label { get; set; } = "";

        [Display(Name = "아이콘 ID")]
        public int? IconId { get; set; }
        public string? IconUrl { get; set; }

        [Display(Name = "색상 코드")]
        public string ColorHex { get; set; } = "#FFFFFF";

        [Display(Name = "정렬 순서")]
        public short SortOrder { get; set; }

        [Display(Name = "활성 여부")]
        public bool IsActive { get; set; }

        [Display(Name = "메타 데이터 (JSON)")]
        public string Meta { get; set; } = "{}";

        [Display(Name = "생성일")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "수정일")]
        public DateTime UpdatedAt { get; set; }
    }
    public sealed class ElementCreateVm
    {
        [Required, RegularExpression("^[a-z0-9_][a-z0-9_-]*$")]
        public string Key { get; set; } = default!;

        [Required] public string Label { get; set; } = default!;
        public int? IconId { get; set; }

        [Required, RegularExpression("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{8})$")]
        public string ColorHex { get; set; } = "#FFFFFF";

        [Range(short.MinValue, short.MaxValue)]
        public short SortOrder { get; set; }

        public string Meta { get; set; } = "{}";

        public List<IconPickItem> Icons { get; set; } = new();
        public string? MetaDescription { get; set; }
        public string? MetaEtc { get; set; }
    }

    public sealed class ElementEditVm
    {
        [Required] public int ElementId { get; set; }
        public string Key { get; set; } = "";   // 읽기 전용 표시용

        [Required] public string Label { get; set; } = default!;
        public int? IconId { get; set; }
        public string? IconUrl { get; set; }

        public string ColorHex { get; set; } = "#FFFFFF";

        public short SortOrder { get; set; }

        public string Meta { get; set; } = "{}";
        public string? MetaDescription { get; set; }
        public string? MetaEtc { get; set; }
        public bool IsActive { get; set; }
        public List<IconPickItem> Icons { get; set; } = new();
    }
    public sealed class IconPickItem
    {
        public int IconId { get; set; }
        public string Key { get; set; } = "";
        public int Version { get; set; }
        public string Url { get; set; } = ""; // cdn/base/icons/{key}.png?v={version}
    }
    public sealed class PickIconModalVm
    {
        public string ModalId { get; set; } = "iconPickerModal";   // 모달 DOM id (페이지마다 유니크)
        public List<IconPickItem> Icons { get; set; } = new();     // 그리드에 뿌릴 아이콘들

        // 선택 결과를 반영할 대상 컨트롤 id
        public string TargetHiddenId { get; set; } = "IconId";         // <input type="hidden">
        public string? TargetPreviewImgId { get; set; } = "iconPreviewImg"; // <img> 미리보기 (없으면 null 허용)
    }
}
