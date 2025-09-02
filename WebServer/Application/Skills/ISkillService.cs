using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Skills
{
    public interface ISkillService
    {
        // 단건 조회 (레벨 미포함)
        Task<SkillDto?> GetAsync(int id, CancellationToken ct);

        // 단건 조회 (레벨 포함) - 필요 시 별도 제공
        Task<SkillWithLevelsDto?> GetWithLevelsAsync(int id, CancellationToken ct);

        // 목록 조회 (필터 + 페이징) → 목록은 요약 DTO로 반환
        Task<IReadOnlyList<SkillListItemDto>> ListAsync(
            SkillType? type,
            int? elementId,
            string? nameContains,
            int page,
            int pageSize,
            CancellationToken ct);

        // 생성
        Task<SkillDto> CreateAsync(CreateSkillRequest req, CancellationToken ct);

        // 수정 (기본 정보: 이름/타입/속성/아이콘)
        Task UpdateAsync(int id, UpdateSkillBasicsRequest req, CancellationToken ct);

        // (옵션) 이름만 경량 수정이 필요하면 별도 메서드 제공
        Task RenameAsync(int id, RenameSkillRequest req, CancellationToken ct);

        // 삭제
        Task DeleteAsync(int id, CancellationToken ct);
    }
}
