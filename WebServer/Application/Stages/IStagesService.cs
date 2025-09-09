using Application.Common.Models;

namespace Application.Stages
{
    public interface ISecurityEventSink
    {
        Task LogAsync(string type, int? userId, object meta, CancellationToken ct);
    }

    public interface IStagesService
    {
        Task<PagedResult<StageSummaryDto>> GetListAsync(StageListFilter filter, CancellationToken ct);
        Task<StageDetailDto?> GetDetailAsync(int id, CancellationToken ct);

        Task<int> CreateAsync(CreateStageRequest req, CancellationToken ct);
        Task UpdateAsync(int id, UpdateStageRequest req, CancellationToken ct);
        Task DeleteAsync(int id, CancellationToken ct);
    }


}
