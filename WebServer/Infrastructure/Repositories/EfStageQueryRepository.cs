using Application.Contents.Stages;
using Application.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Infrastructure.Repositories
{
    public sealed class EfStageQueryRepository : IStageQueryRepository
    {
        private readonly GameDBContext _db;

        public EfStageQueryRepository(GameDBContext db) => _db = db;

        // 목록 (페이징 + 필터 + 간단 검색)
        public async Task<Application.Common.Models.PagedResult<StageSummaryDto>> GetListAsync(
       StageListFilter filter, CancellationToken ct)
        {
            var q = _db.Stages.AsNoTracking();

            if (filter.Chapter.HasValue)
                q = q.Where(s => s.Chapter == filter.Chapter.Value);

            if (filter.IsActive.HasValue)
                q = q.Where(s => s.IsActive == filter.IsActive.Value);

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var term = filter.Search.Trim().ToLower();
                q = q.Where(s =>
                    ((EF.Property<string?>(s, "Name") ?? "").ToLower()).Contains(term) ||
                    (s.Chapter.ToString() + "-" + s.StageNumber.ToString()).ToLower().Contains(term));
            }

            var total = await q.CountAsync(ct);

            var page = filter.Page <= 0 ? 1 : filter.Page;
            var size = filter.PageSize <= 0 ? 20 : filter.PageSize;

            var items = await q
                .OrderBy(s => s.Chapter).ThenBy(s => s.StageNumber).ThenBy(s => s.Id)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(s => new StageSummaryDto(
                    s.Id,
                    s.Chapter,
                    s.StageNumber,
                    EF.Property<string?>(s, "Name"),
                    s.RecommendedPower,
                    s.StaminaCost,
                    s.IsActive))
                .ToListAsync(ct);

            return new Application.Common.Models.PagedResult<StageSummaryDto>(items, total, page, size);
        }

        // 상세 (그래프 전체 로드 → DTO 매핑)
        public async Task<StageDetailDto?> GetDetailAsync(int id, CancellationToken ct)
        {
            var entity = await _db.Stages
                .AsNoTracking()
                .Include(s => s.Waves).ThenInclude(w => w.Enemies)
                .Include(s => s.Drops)
                .Include(s => s.FirstRewards)
                .Include(s => s.Requirements)
                .FirstOrDefaultAsync(s => s.Id == id, ct);

            return entity is null ? null : entity.ToDetailDto();
        }

        // Export: 챕터 필터로 여러 개 상세 그래프 한번에
        public async Task<IReadOnlyList<StageDetailDto>> ExportByChapterAsync(int? chapter, CancellationToken ct)
        {
            var q = _db.Stages.AsNoTracking();

            if (chapter.HasValue)
                q = q.Where(s => s.Chapter == chapter.Value);

            var entities = await q
                .OrderBy(s => s.Chapter).ThenBy(s => s.StageNumber).ThenBy(s => s.Id)
                .Include(s => s.Waves).ThenInclude(w => w.Enemies)
                .Include(s => s.Drops)
                .Include(s => s.FirstRewards)
                .Include(s => s.Requirements)
                .ToListAsync(ct);

            return entities.Select(s => s.ToDetailDto()).ToList();
        }
    }
}
