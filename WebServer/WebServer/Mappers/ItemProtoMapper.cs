using Application.Items;
using Contracts.Protos;
using Domain.Enum;
using Google.Protobuf.WellKnownTypes;
using System.Text.Json;

namespace WebServer.Mappers
{
    public static class ItemProtoMapper
    {
        private static BindTypePb ToPb(this BindType x) => (BindTypePb)x;
        private static ItemEffectScopePb ToPb(this ItemEffectScope x) => (ItemEffectScopePb)x;
        private static ItemPriceTypePb ToPb(this ItemPriceType x) => (ItemPriceTypePb)x;
        private static string JsonToString(JsonDocument? doc) => doc?.RootElement.GetRawText() ?? string.Empty;

        // 요약 변환 (확장 메서드)
        public static ItemSummaryMessage ToSummaryPb(this ItemDto x, Func<int?, string?> iconUrlFactory)
            => new ItemSummaryMessage
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                TypeId = x.TypeId,
                RarityId = x.RarityId,
                IconId = x.IconId ?? 0,
                IsActive = x.IsActive,
                IconUrl = iconUrlFactory(x.IconId) ?? string.Empty,
            };

        // 상세 변환 (확장 메서드)
        public static ItemMessage ToDetailPb(this ItemDto x,
            Func<int?, string?> iconUrlFactory,
            Func<int?, string?> portraitUrlFactory)
        {
            var m = new ItemMessage
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description ?? string.Empty,
                TypeId = x.TypeId,
                RarityId = x.RarityId,
                IconId = x.IconId ?? 0,
                PortraitId = x.PortraitId ?? 0,
                Stackable = x.Stackable,
                MaxStack = x.MaxStack,
                BindType = x.BindType.ToPb(),
                Tradable = x.Tradable,
                DurabilityMax = x.DurabilityMax ?? 0,
                Weight = x.Weight,
                IsActive = x.IsActive,
                MetaJson = JsonToString(x.Meta),
                CreatedBy = x.CreatedBy ?? string.Empty,
                CreatedAtMs = x.CreatedAt.ToUnixTimeMilliseconds(),
                UpdatedAtMs = x.UpdatedAt.ToUnixTimeMilliseconds(),
                IconUrl = iconUrlFactory(x.IconId) ?? string.Empty,
                PortraitUrl = portraitUrlFactory(x.PortraitId) ?? string.Empty,
            };

            if (x.Tags?.Length > 0) m.Tags.AddRange(x.Tags);

            if (x.Stats is { Count: > 0 })
                m.Stats.AddRange(x.Stats.Select(s => new ItemStatMessage
                {
                    Id = s.Id,
                    StatId = s.StatId,
                    Value = (double)s.Value
                }));

            if (x.Effects is { Count: > 0 })
                m.Effects.AddRange(x.Effects
                    .OrderBy(e => e.SortOrder)
                    .Select(e => new ItemEffectMessage
                    {
                        Id = e.Id,
                        Scope = e.Scope.ToPb(),
                        PayloadJson = e.Payload.RootElement.GetRawText(),
                        SortOrder = e.SortOrder
                    }));

            if (x.Prices is { Count: > 0 })
                m.Prices.AddRange(x.Prices.Select(p => new ItemPriceMessage
                {
                    Id = p.Id,
                    CurrencyId = p.CurrencyId,
                    PriceType = p.PriceType.ToPb(),
                    Price = p.Price
                }));

            return m;
        }
    }
}
