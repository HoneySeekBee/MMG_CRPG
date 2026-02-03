using Domain.Entities.Shop;

namespace Application.Repositories
{
    public interface IShopRepository
    {
        Task<Shop?> GetByIdAsync(int id, CancellationToken ct);

        // Products 포함 조회 (상점 수정 시 사용)
        Task<Shop?> GetByIdWithProductsAsync(int id, CancellationToken ct);

        // 코드 중복 체크 (수정 시 excludeId로 자기 자신 제외)
        Task<bool> ExistsCodeAsync(string code, int? excludeId, CancellationToken ct);

        Task AddAsync(Shop entity, CancellationToken ct);

        void Remove(Shop entity);

        Task<int> SaveChangesAsync(CancellationToken ct);
    }
}
