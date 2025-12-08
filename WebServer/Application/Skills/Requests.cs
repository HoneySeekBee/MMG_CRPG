using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Application.Skills
{   // 생성
    public sealed class CreateSkillRequest
    {
        // [1] 기본 정보 
        public string Name { get; set; } = "";
        public int IconId { get; set; }

        // [2] 상세 정보 
        public SkillType Type { get; set; } = SkillType.Unknown;
        public int ElementId { get; set; }
        public SkillTargetingType TargetingType { get; set; } = SkillTargetingType.None;
        public AoeShapeType AoeShape { get; set; } = AoeShapeType.None;
        public TargetSideType TargetSide { get; set; } = TargetSideType.Team;

        // [3] 기타 정보 
        public bool? IsActive { get; init; }
        public string[]? Tag { get; set; } // null = 미설정
        public JsonNode? BaseInfo { get; set; } // jsonb 매핑

        // 필요 시 기본 메타/베이스 설정을 DTO로 받으려면 여기에 추가 (ex: Trigger/Cooldown 등)
    }

    // 기본 정보 수정 (이름/타입/아이콘)
    public sealed class UpdateSkillBasicsRequest
    {
        public string Name { get; set; } = "";
        public int IconId { get; set; }
    }
    // 전투 속성 업데이트(액티브 여부/타게팅/범위/대상 진영)
    // -> PUT /skills/{id}/combat
    public sealed class UpdateSkillCombatRequest
    {
        public SkillType Type { get; set; } = SkillType.Unknown;
        public int ElementId { get; set; }
        public SkillTargetingType TargetingType { get; set; } = SkillTargetingType.None;
        public AoeShapeType AoeShape { get; set; } = AoeShapeType.None;
        public TargetSideType TargetSide { get; set; } = TargetSideType.None;
        public bool IsActive { get; set; }
    }
    public sealed class PatchSkillMetaRequest
    {
        public string[]? Tag { get; init; }
        public JsonNode? BaseInfo { get; init; }   // jsonb 그대로 교체(merge가 필요하면 로직 추가)
        public bool NormalizeTags { get; init; } = true;
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

    // 삭제
    public sealed class DeleteSkillRequest
    {
        public int SkillId { get; set; }
    }
}
