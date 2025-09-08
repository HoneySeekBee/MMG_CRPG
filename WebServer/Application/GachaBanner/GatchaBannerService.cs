using Application.Repositories;
using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.GachaBanner
{
    public sealed class GachaBannerService : IGachaBannerService
    {
        private readonly IGachaBannerRepository _repo;

        public GachaBannerService(IGachaBannerRepository repo) => _repo = repo;

        public async Task<GachaBannerDto?> GetAsync(int id, CancellationToken ct = default)
            => (await _repo.GetByIdAsync(id, ct))?.ToDto();

        public async Task<GachaBannerDto?> GetByKeyAsync(string key, CancellationToken ct = default)
            => (await _repo.GetByKeyAsync(key, ct))?.ToDto();

        public async Task<IReadOnlyList<GachaBannerDto>> ListLiveAsync(int take = 10, CancellationToken ct = default)
        {
            var list = await _repo.ListLiveAsync(DateTimeOffset.UtcNow, take, ct);
            return list.Select(x => x.ToDto()).ToList();
        }

        public async Task<(IReadOnlyList<GachaBannerDto> Items, int Total)> SearchAsync(
            QueryGachaBannersRequest req, CancellationToken ct = default)
        {
            var (items, total) = await _repo.SearchAsync(req.Keyword, req.Skip, req.Take, ct);
            return (items.Select(x => x.ToDto()).ToList(), total);
        }

        public async Task<GachaBannerDto> CreateAsync(CreateGachaBannerRequest req, CancellationToken ct = default)
        {
            var entity = Domain.Entities.GachaBanner.Create(
                key: req.Key,
                title: req.Title,
                gachaPoolId: req.GachaPoolId,
                startsAt: req.StartsAt,
                endsAt: req.EndsAt,
                subtitle: req.Subtitle,
                portraitId: req.PortraitId,
                priority: req.Priority,
                status: req.Status,
                isActive: req.IsActive
            );

            await _repo.AddAsync(entity, ct);
            await _repo.SaveChangesAsync(ct);
            return entity.ToDto();
        }
        public async Task<GachaBannerDto> UpdateAsync(UpdateGachaBannerRequest req, CancellationToken ct = default)
        {
            var entity = await _repo.GetByIdAsync(req.Id, ct)
                ?? throw new KeyNotFoundException($"Banner {req.Id} not found");

            entity.Rename(req.Title, req.Subtitle);
            entity.LinkPortrait(req.PortraitId);
            entity.LinkPool(req.GachaPoolId);
            entity.Reschedule(req.StartsAt, req.EndsAt);
            entity.SetPriority(req.Priority);
            entity.SetStatus(req.Status);

            if (entity.IsActive != req.IsActive)
            {
                if (req.IsActive) entity.Activate();
                else entity.Deactivate();
            }

            await _repo.UpdateAsync(entity, ct);
            await _repo.SaveChangesAsync(ct);
            return entity.ToDto();
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            await _repo.DeleteAsync(id, ct);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task SetStatusAsync(int id, GachaBannerStatus status, CancellationToken ct = default)
        {
            var e = await _repo.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"Banner {id} not found");
            e.SetStatus(status);
            await _repo.UpdateAsync(e, ct);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task SetActiveAsync(int id, bool isActive, CancellationToken ct = default)
        {
            var e = await _repo.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"Banner {id} not found");
            if (isActive) e.Activate(); else e.Deactivate();
            await _repo.UpdateAsync(e, ct);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task RescheduleAsync(int id, DateTimeOffset startsAt, DateTimeOffset? endsAt, CancellationToken ct = default)
        {
            var e = await _repo.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"Banner {id} not found");
            e.Reschedule(startsAt, endsAt);
            await _repo.UpdateAsync(e, ct);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task SetPriorityAsync(int id, short priority, CancellationToken ct = default)
        {
            var e = await _repo.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"Banner {id} not found");
            e.SetPriority(priority);
            await _repo.UpdateAsync(e, ct);
            await _repo.SaveChangesAsync(ct);
        }
    }
}
