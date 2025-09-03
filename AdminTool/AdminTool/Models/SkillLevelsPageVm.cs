using Application.SkillLevels;
using Domain.Enum;

namespace AdminTool.Models
{
    public sealed class SkillLevelsPageVm
    {
        // 헤더/버튼에 필요한 스킬 기본 정보
        public int SkillId { get; init; }
        public string SkillName { get; init; } = "";
        public string? IconUrl { get; init; }          // 선택: 아이콘 보여줄 거면

        public SkillType ParentType { get; init; }     // openCreateLevel 인자
        public bool IsPassive { get; init; }           // openCreateLevel 인자 (IsActive의 반대)

        // 목록
        public IReadOnlyList<SkillLevelDto> Items { get; init; } = Array.Empty<SkillLevelDto>();

        // 편의 값(뷰에서 새 레벨 기본값 계산에 사용)
        public int NextLevel => Items.Count == 0 ? 1 : (Items.Max(x => x.Level) + 1);

        // 모달/호스트 등 UI 구성요소
        public LevelEditModalVm Modal { get; init; } = new("levelEditModal", 0, "levelsHost");

        // 플래그(보기 전용 등 필요하면)
        public bool ReadOnly { get; init; }
    }

    public sealed class SkillLevelsVm
    {
        public int SkillId { get; init; }
        public IReadOnlyList<SkillLevelDto> Items { get; init; } = new List<SkillLevelDto>();
    }
    public sealed record LevelEditModalVm(
        string ModalId,        // 모달 DOM id (ex: "levelEditModal")
        int SkillId,           // 현재 스킬 ID
        string LevelsHostId    // 목록을 넣어둔 컨테이너 id (ex: "levelsHost")
    );
    public sealed class SkillLevelFormVm
    {
        public int SkillId { get; init; }
        public int Level { get; set; } // Create 시 수정가능, Update 시 read-only 처리

        public string? Values { get; set; }      // {"scale":2.0,"burn":{"dmg":20,"dur":5}}
        public string? Description { get; set; }
        public string? Materials { get; set; }   // {"501":3,"777":1}
        public int CostGold { get; set; }
        public bool IsEdit { get; init; }
        public SkillType ParentType { get; set; }
        public bool IsPassive { get; set; }
    }
}
