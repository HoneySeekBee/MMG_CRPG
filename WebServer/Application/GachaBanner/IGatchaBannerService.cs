using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.GachaBanner
{
    public interface IGachaBannerService
    {
        Task<GachaBannerDto?> GetAsync(int id, CancellationToken ct = default);
        Task<GachaBannerDto?> GetByKeyAsync(string key, CancellationToken ct = default);

        Task<IReadOnlyList<GachaBannerDto>> ListLiveAsync(int take = 10, CancellationToken ct = default);
        Task<(IReadOnlyList<GachaBannerDto> Items, int Total)> SearchAsync(QueryGachaBannersRequest req, CancellationToken ct = default);

        Task<GachaBannerDto> CreateAsync(CreateGachaBannerRequest req, CancellationToken ct = default);
        Task<GachaBannerDto> UpdateAsync(UpdateGachaBannerRequest req, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);

        Task SetStatusAsync(int id, Domain.Enum.GachaBannerStatus status, CancellationToken ct = default);
        Task SetActiveAsync(int id, bool isActive, CancellationToken ct = default);
        Task RescheduleAsync(int id, DateTimeOffset startsAt, DateTimeOffset? endsAt, CancellationToken ct = default);
        Task SetPriorityAsync(int id, short priority, CancellationToken ct = default);
    }
}
