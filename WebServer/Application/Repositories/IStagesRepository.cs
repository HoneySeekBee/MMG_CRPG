using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common.Models;
using Domain.Entities.Contents;
using Application.Contents.Stages;

namespace Application.Repositories
{
    public interface IStagesRepository
    {
        /// <summary>그래프 전체 로드 (Waves/Enemies/Drops/FirstRewards/Requirements 포함)</summary>
        Task<Stage?> LoadGraphAsync(int id, CancellationToken ct);

        /// <summary>동등성/중복 체크에 사용할 최소 정보만 로드 (성능 고려)</summary>
        Task<Stage?> GetBasicAsync(int id, CancellationToken ct);

        /// <summary>(Chapter, Order) 중복 여부</summary>
        Task<bool> ExistsChapterOrderAsync(int chapter, int order, int? excludeId, CancellationToken ct);

        /// <summary>(옵션) Chapter+Name 유니크를 쓴다면 중복 체크</summary>
        Task<bool> ExistsChapterNameAsync(int chapter, string? name, int? excludeId, CancellationToken ct);

        /// <summary>추가(그래프 삽입)</summary>
        Task AddAsync(Stage entity, CancellationToken ct);

        /// <summary>삭제(그래프 삭제). 유저 진행도는 정책대로 보존/제외</summary>
        void Remove(Stage entity);

        /// <summary>변경 저장</summary>
        Task SaveAsync(CancellationToken ct);
    }

    /// <summary>
    /// 읽기/검색 전용 쿼리 저장소. DTO로 투영하여 반환.
    /// </summary>
    public interface IStageQueryRepository
    {
        /// <summary>목록(페이징+필터)</summary>
        Task<Common.Models.PagedResult<StageSummaryDto>> GetListAsync(StageListFilter filter, CancellationToken ct);

        /// <summary>상세(그래프 DTO)</summary>
        Task<StageDetailDto?> GetDetailAsync(int id, CancellationToken ct);

        /// <summary>Export용: 챕터 범위/필터로 상세 그래프 일괄 조회</summary>
        Task<IReadOnlyList<StageDetailDto>> ExportByChapterAsync(int? chapter, CancellationToken ct);
    }
}
