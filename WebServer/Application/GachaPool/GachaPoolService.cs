using Application.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.GachaPool
{
    public sealed class GachaPoolService : IGachaPoolService
    {
        private readonly IGachaPoolRepository _repo;

        public GachaPoolService(IGachaPoolRepository repo) => _repo = repo;

        // ───────────── 조회 ─────────────
        public async Task<GachaPoolDto?> GetAsync(int poolId, CancellationToken ct = default)
            => (await _repo.GetByIdAsync(poolId, ct))?.ToDto();

        

        public async Task<(IReadOnlyList<GachaPoolDto> Items, int Total)> SearchAsync(
            QueryGachaPoolsRequest req, CancellationToken ct = default)
        {
            var (items, total) = await _repo.SearchAsync(req.Keyword, req.Skip, req.Take, ct);
            return (items.Select(x => x.ToDto()).ToList(), total);
        }

        public async Task<IReadOnlyList<GachaPoolDto>> ListAsync(int take = 100, CancellationToken ct = default)
        {
            var items = await _repo.ListAsync(take, ct);
            return items.Select(x => x.ToDto()).ToList();
        }

        // ───────────── CUD ─────────────
        public async Task<GachaPoolDto> CreateAsync(CreateGachaPoolRequest req, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(req.Name))
                throw new ArgumentException("name is required", nameof(req.Name));

            var e = Domain.Entities.GachaPool.Create(
                name: req.Name,
                scheduleStart: req.ScheduleStart,
                scheduleEnd: req.ScheduleEnd,
                pityJson: req.PityJson,
                tablesVersion: req.TablesVersion,
                configJson: req.ConfigJson
            );

            await _repo.AddAsync(e, ct);
            await _repo.SaveChangesAsync(ct);
            return e.ToDto();
        }

        public async Task<GachaPoolDto> UpdateAsync(UpdateGachaPoolRequest req, CancellationToken ct = default)
        {
            var e = await _repo.GetByIdAsync(req.PoolId, ct)
                ?? throw new KeyNotFoundException($"Pool {req.PoolId} not found");

            e.Rename(req.Name);
            e.Reschedule(req.ScheduleStart, req.ScheduleEnd);
            e.SetPityJson(req.PityJson);
            e.SetTablesVersion(req.TablesVersion);
            e.SetConfigJson(req.ConfigJson);

            await _repo.UpdateAsync(e, ct);
            await _repo.SaveChangesAsync(ct);
            return e.ToDto();
        }
        public async Task<GachaPoolDetailDto?> GetDetailAsync(int poolId, CancellationToken ct = default)
        {
            var e = await _repo.GetByIdAsync(poolId, ct);
            if (e is null) return null;

            // Entries가 필요하다면 레포를 Include 방식으로 바꿔도 됨.
            // 여기서는 ReplaceEntries 시 항상 DB에 존재하므로 다시 한 번 가져와도 OK
            var withEntries = await _repo.GetWithEntriesAsync(poolId, ct) ?? e;
            return withEntries.ToDetailDto();
        }

        public async Task DeleteAsync(int poolId, CancellationToken ct = default)
        {
            await _repo.DeleteAsync(poolId, ct);
            await _repo.SaveChangesAsync(ct);
        }

        // ───────────── 엔트리 교체 ─────────────
        public async Task ReplaceEntriesAsync(UpsertGachaPoolEntriesRequest req, CancellationToken ct = default)
        {
            if (req.Entries is null) throw new ArgumentNullException(nameof(req.Entries));
            if (req.Entries.Count == 0) // 빈 확률표 허용 여부는 정책에 따라
                throw new ArgumentException("Entries must not be empty.", nameof(req.Entries));

            // 유효성(가중치>0, 중복 캐릭터 금지)
            if (req.Entries.Any(x => x.Weight <= 0))
                throw new ArgumentException("All weights must be positive.", nameof(req.Entries));
            if (req.Entries.Select(x => x.CharacterId).Distinct().Count() != req.Entries.Count)
                throw new ArgumentException("Duplicate character entries.", nameof(req.Entries));

            var entries = req.Entries.Select(x => x.ToEntity());
            await _repo.ReplaceEntriesAsync(req.PoolId, entries, ct);
        }
    }
}
