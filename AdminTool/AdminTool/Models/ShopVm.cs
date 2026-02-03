using System.ComponentModel.DataAnnotations;

namespace AdminTool.Models
{
    // ───── Index 필터 ─────
    public sealed class ShopListFilterVm
    {
        public int? ShopType { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    // ───── Index 행 ─────
    public sealed class ShopVm
    {
        public int Id { get; init; }
        public string Code { get; init; } = "";
        public string Name { get; init; } = "";
        public int ShopType { get; init; }
        public DateTimeOffset? StartsAt { get; init; }
        public DateTimeOffset? EndsAt { get; init; }
        public bool IsActive { get; init; }
        public int SortOrder { get; init; }

        public string ShopTypeName => ShopType == 0 ? "General" : "TimeLimited";
    }

    // ───── 상점 생성/수정 폼 ─────
    public sealed class ShopEditVm
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Code { get; set; } = "";

        [Required, StringLength(200)]
        public string Name { get; set; } = "";

        public int ShopType { get; set; }
        public DateTimeOffset? StartsAt { get; set; }
        public DateTimeOffset? EndsAt { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; }

        // Edit 페이지에서 하단 상품 목록 표시용
        public List<ShopProductVm> Products { get; set; } = new();
    }

    // ───── 상품 행 ─────
    public sealed class ShopProductVm
    {
        public int Id { get; init; }
        public int ShopId { get; init; }
        public int ItemId { get; init; }
        public int CurrencyId { get; init; }
        public long Price { get; init; }
        public int QuantityPerPurchase { get; init; }
        public int? DailyLimit { get; init; }
        public int? WeeklyLimit { get; init; }
        public int? TotalLimit { get; init; }
        public int SortOrder { get; init; }
        public bool IsActive { get; init; }
    }

    // ───── 상품 추가/수정 폼 ─────
    public sealed class ShopProductEditVm
    {
        public int ProductId { get; set; }
        public int ItemId { get; set; }
        public int CurrencyId { get; set; }
        public long Price { get; set; }
        public int QuantityPerPurchase { get; set; } = 1;
        public int? DailyLimit { get; set; }
        public int? WeeklyLimit { get; set; }
        public int? TotalLimit { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }

    // ───── 구매 기록 필터 ─────
    public sealed class PurchaseLogFilterVm
    {
        public int? UserId { get; set; }
        public int? ShopProductId { get; set; }
        public DateTimeOffset? From { get; set; }
        public DateTimeOffset? To { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

    // ───── 구매 기록 행 ─────
    public sealed class PurchaseLogVm
    {
        public long Id { get; init; }
        public int UserId { get; init; }
        public int ShopProductId { get; init; }
        public int Quantity { get; init; }
        public long PricePaid { get; init; }
        public string CurrencyCode { get; init; } = "";
        public DateTimeOffset PurchasedAt { get; init; }
    }
}
