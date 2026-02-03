using Application.Common.Models;

namespace Application.Shop
{
    public interface IShopService
    {
        // 상점
        Task<Common.Models.PagedResult<ShopDto>> GetAllAsync(ShopListFilter filter, CancellationToken ct);
        Task<ShopDetailDto> GetByIdAsync(int id, CancellationToken ct);
        Task<int> CreateAsync(CreateShopRequest req, CancellationToken ct);
        Task UpdateAsync(int id, UpdateShopRequest req, CancellationToken ct);
        Task DeleteAsync(int id, CancellationToken ct);

        // 상품
        Task<ShopProductDto> AddProductAsync(int shopId, CreateShopProductRequest req, CancellationToken ct);
        Task UpdateProductAsync(int shopId, int productId, UpdateShopProductRequest req, CancellationToken ct);
        Task DeleteProductAsync(int shopId, int productId, CancellationToken ct);

        // 구매 기록
        Task<Common.Models.PagedResult<PurchaseLogDto>> GetPurchaseLogsAsync(PurchaseLogFilter filter, CancellationToken ct);
    }
}
