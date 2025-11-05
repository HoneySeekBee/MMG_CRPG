using Application.Repositories;
using Domain.Entities.Contents;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public sealed class EfStagesRepository : IStagesRepository
    {
        private readonly GameDBContext _db;

        public EfStagesRepository(GameDBContext db) => _db = db;

        /// <summary>
        /// 그래프 전체 로드 (Waves/Enemies/Drops/FirstRewards/Requirements)
        /// </summary>
        public Task<Stage?> LoadGraphAsync(int id, CancellationToken ct)
            => _db.Stages
                  .Include(s => s.Waves)
                      .ThenInclude(w => w.Enemies)
                  .Include(s => s.Drops)
                  .Include(s => s.FirstRewards)
                  .Include(s => s.Requirements)
                  .FirstOrDefaultAsync(s => s.Id == id, ct);

        /// <summary>
        /// 최소 필드만 로드(존재/중복 체크 등 경량 용도)
        /// </summary>
        public Task<Stage?> GetBasicAsync(int id, CancellationToken ct)
            => _db.Stages
                  .AsNoTracking()
                  .Select(s => new StageProxy
                  {
                      Id = s.Id,
                      Chapter = s.Chapter,
                      StageNumber = s.StageNumber,
                      Name = EF.Property<string?>(s, "Name") // 존재 시만 가져옴
                  })
                  .Where(s => s.Id == id)
                  .Select(p => new Stage(p.Chapter, p.StageNumber, default, default, true, null) // 최소 생성자
                  {
                      // Id를 세팅하려면 EF 프록시 대신 실제 엔티티를 로드해야 하지만,
                      // 여기서는 존재 확인용이라 null/값 체크만 충분.
                  })
                  .FirstOrDefaultAsync(ct);

        /// <summary>
        /// (Chapter, Order) 유니크 중복 체크
        /// </summary>
        public Task<bool> ExistsChapterOrderAsync(int chapter, int stageNumber, int? excludeId, CancellationToken ct)
        {
            var q = _db.Stages.AsNoTracking().Where(s => s.Chapter == chapter && s.StageNumber == stageNumber);
            if (excludeId.HasValue) q = q.Where(s => s.Id != excludeId.Value);
            return q.AnyAsync(ct);
        }

        /// <summary>
        /// (옵션) (Chapter, Name) 유니크 중복 체크 (Name null/empty는 무시)
        /// </summary>
        public Task<bool> ExistsChapterNameAsync(int chapter, string? name, int? excludeId, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(name)) return Task.FromResult(false);

            // Name 컬럼이 없을 수도 있으니 EF.Property 사용
            var q = _db.Stages.AsNoTracking()
                    .Where(s => s.Chapter == chapter && EF.Property<string?>(s, "Name") == name);

            if (excludeId.HasValue) q = q.Where(s => s.Id != excludeId.Value);
            return q.AnyAsync(ct);
        }

        /// <summary>
        /// 그래프 추가
        /// </summary>
        public async Task AddAsync(Stage entity, CancellationToken ct)
        {
            await _db.Stages.AddAsync(entity, ct);
        }

        /// <summary>
        /// 그래프 제거 (자식은 FK Cascade)
        /// </summary>
        public void Remove(Stage entity)
        {
            _db.Stages.Remove(entity);
        }

        /// <summary>
        /// 변경 저장
        /// </summary>
        public Task SaveAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);

        // 내부용 경량 프록시(AsNoTracking select용)
        private sealed class StageProxy
        {
            public int Id { get; set; }
            public int Chapter { get; set; }
            public int StageNumber { get; set; }
            public string? Name { get; set; }
        }
    }
}
