using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Skills
{   // 생성
    public sealed class CreateSkillRequest
    {
        public string Name { get; set; } = "";
        public SkillType Type { get; set; } = SkillType.Unknown;
        public int ElementId { get; set; }
        public int IconId { get; set; }
        // 필요 시 기본 메타/베이스 설정을 DTO로 받으려면 여기에 추가 (ex: Trigger/Cooldown 등)
    }

    // 기본 정보 수정 (이름/타입/속성/아이콘)
    public sealed class UpdateSkillBasicsRequest
    {
        public string Name { get; set; } = "";
        public SkillType Type { get; set; } = SkillType.Unknown;
        public int ElementId { get; set; }
        public int IconId { get; set; }
    }

    // 이름만 수정(경량)
    public sealed class RenameSkillRequest
    {
        public string Name { get; set; } = "";
    }

    // 단건 조회
    public sealed class GetSkillRequest
    {
        public int SkillId { get; set; }
        public bool IncludeLevels { get; set; } = true;
    }

    // 목록 조회 + 필터
    public sealed class ListSkillsRequest
    {
        public SkillType? Type { get; set; }
        public int? ElementId { get; set; }
        public string? NameContains { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    // 삭제
    public sealed class DeleteSkillRequest
    {
        public int SkillId { get; set; }
    }
}
