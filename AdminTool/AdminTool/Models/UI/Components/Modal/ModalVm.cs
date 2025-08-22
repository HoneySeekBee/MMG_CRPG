namespace AdminTool.Models.UI.Components.Modal
{
    public record ModalVm(
    string Id,
    string? Title = null,
    string Size = "",                 // "", "modal-lg", "modal-xl", "modal-sm"
    string? HeaderPartial = null,     // 커스텀 헤더가 필요할 때
    object? HeaderModel = null,
    string? BodyPartial = null,       // 본문을 Partial로 주입
    object? BodyModel = null,
    string? FooterPartial = null,     // 커스텀 푸터(버튼들) 주입
    object? FooterModel = null,
    bool StaticBackdrop = false,      // 외부 클릭으로 닫힘 금지
    bool Centered = true,             // 가운데 정렬
    bool Scrollable = true,           // 스크롤 가능
    IDictionary<string, string>? Attr = null // 추가 data-*, aria-*, class 등의 속성
);
}
