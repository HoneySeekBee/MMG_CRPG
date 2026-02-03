using Application.Common.Models;
using Application.Repositories;
using Domain.Entities.Shop;
using Microsoft.Extensions.Logging;

namespace Application.Shop
{
    public sealed class ShopService : IShopService
    {
        private readonly IShopRepository _repo;
        private readonly IShopQueryRepository _queryRepo;
        private readonly IShopProductRepository _productRepo;
        private readonly IUserPurchaseLogQueryRepository _logQueryRepo;
        private readonly ILogger<ShopService> _log;

        public ShopService(
            IShopRepository repo,
            IShopQueryRepository queryRepo,
            IShopProductRepository productRepo,
            IUserPurchaseLogQueryRepository logQueryRepo,
            ILogger<ShopService> log)
        {
            _repo = repo;
            _queryRepo = queryRepo;
            _productRepo = productRepo;
            _logQueryRepo = logQueryRepo;
            _log = log;
        }

        // ───── 상점 ─────

        public async Task<Common.Models.PagedResult<ShopDto>> GetAllAsync(ShopListFilter filter, CancellationToken ct)
        {
            var (items, total) = await _queryRepo.GetPagedAsync(
                filter.ShopType, filter.IsActive, filter.Search,
                filter.Page, filter.PageSize, ct);

            return new Common.Models.PagedResult<ShopDto>(
                items.Select(x => x.ToDto()).ToList(),
                filter.Page, filter.PageSize, total);
        }

        public async Task<ShopDetailDto> GetByIdAsync(int id, CancellationToken ct)
        {
            var shop = await _queryRepo.GetDetailAsync(id, ct)
                ?? throw new KeyNotFoundException("Shop not found.");
            return shop.ToDetailDto();
        }

        public async Task<int> CreateAsync(CreateShopRequest req, CancellationToken ct)
        {
            if (await _repo.ExistsCodeAsync(req.Code, null, ct))
                throw new InvalidOperationException("DUPLICATE_SHOP_CODE");

            var shop = new Domain.Entities.Shop.Shop(
                req.Code, req.Name, req.ShopType,
                req.StartsAt, req.EndsAt, req.IsActive, req.SortOrder);

            await _repo.AddAsync(shop, ct);
            await _repo.SaveChangesAsync(ct);

            _log.LogInformation("Shop created {ShopId} code={Code}", shop.Id, shop.Code);
            return shop.Id;
        }

        public async Task UpdateAsync(int id, UpdateShopRequest req, CancellationToken ct)
        {
            var shop = await _repo.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException("Shop not found.");

            shop.Update(req.Name, req.ShopType, req.StartsAt, req.EndsAt, req.IsActive, req.SortOrder);
            await _repo.SaveChangesAsync(ct);

            _log.LogInformation("Shop updated {ShopId}", id);
        }

        public async Task DeleteAsync(int id, CancellationToken ct)
        {
            var shop = await _repo.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException("Shop not found.");

            _repo.Remove(shop);
            await _repo.SaveChangesAsync(ct);

            _log.LogInformation("Shop deleted {ShopId}", id);
        }

        // ───── 상품 ─────

        public async Task<ShopProductDto> AddProductAsync(
            int shopId, CreateShopProductRequest req, CancellationToken ct)
        {
            // 상점 존재 확인
            _ = await _repo.GetByIdAsync(shopId, ct)
                ?? throw new KeyNotFoundException("Shop not found.");

            var product = new ShopProduct(
                shopId, req.ItemId, req.CurrencyId, req.Price,
                req.QuantityPerPurchase, req.DailyLimit, req.WeeklyLimit, req.TotalLimit,
                req.SortOrder, req.IsActive);

            await _productRepo.AddAsync(product, ct);
            await _productRepo.SaveChangesAsync(ct);

            _log.LogInformation("ShopProduct created {ProductId} in shop {ShopId}", product.Id, shopId);
            return product.ToDto();
        }

        public async Task UpdateProductAsync(
            int shopId, int productId, UpdateShopProductRequest req, CancellationToken ct)
        {
            var product = await _productRepo.GetByIdAsync(productId, ct)
                ?? throw new KeyNotFoundException("Product not found.");

            if (product.ShopId != shopId)
                throw new InvalidOperationException("PRODUCT_SHOP_MISMATCH");

            product.Update(
                req.ItemId, req.CurrencyId, req.Price, req.QuantityPerPurchase,
                req.DailyLimit, req.WeeklyLimit, req.TotalLimit,
                req.SortOrder, req.IsActive);

            await _productRepo.SaveChangesAsync(ct);

            _log.LogInformation("ShopProduct updated {ProductId}", productId);
        }

        public async Task DeleteProductAsync(int shopId, int productId, CancellationToken ct)
        {
            var product = await _productRepo.GetByIdAsync(productId, ct)
                ?? throw new KeyNotFoundException("Product not found.");

            if (product.ShopId != shopId)
                throw new InvalidOperationException("PRODUCT_SHOP_MISMATCH");

            _productRepo.Remove(product);
            await _productRepo.SaveChangesAsync(ct);

            _log.LogInformation("ShopProduct deleted {ProductId}", productId);
        }

        // ───── 구매 기록 ─────

        public async Task<Common.Models.PagedResult<PurchaseLogDto>> GetPurchaseLogsAsync(
            PurchaseLogFilter filter, CancellationToken ct)
        {
            var (items, total) = await _logQueryRepo.GetLogsPagedAsync(
                filter.UserId, filter.ShopProductId, filter.From, filter.To,
                filter.Page, filter.PageSize, ct);

            return new Common.Models.PagedResult<PurchaseLogDto>(
                items.Select(x => x.ToDto()).ToList(),
                filter.Page, filter.PageSize, total);
        }
    }
}
