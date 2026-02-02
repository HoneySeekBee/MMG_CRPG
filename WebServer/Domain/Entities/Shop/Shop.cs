using Domain.Enum;
using System;
using System.Collections.Generic;

namespace Domain.Entities.Shop
{
    public sealed class Shop
    {
        public int Id { get; private set; }
        public string Code { get; private set; } = "";
        public string Name { get; private set; } = "";
        public ShopType ShopType { get; private set; }
        public DateTimeOffset? StartsAt { get; private set; }
        public DateTimeOffset? EndsAt { get; private set; }
        public bool IsActive { get; private set; }
        public int SortOrder { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset UpdatedAt { get; private set; }

        private readonly List<ShopProduct> _products = new();
        public IReadOnlyList<ShopProduct> Products => _products;

        private Shop() { }

        public Shop(
            string code,
            string name,
            ShopType shopType,
            DateTimeOffset? startsAt = null,
            DateTimeOffset? endsAt = null,
            bool isActive = true,
            int sortOrder = 0)
        {
            if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Code is required.", nameof(code));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));

            Code = code.Trim();
            Name = name.Trim();
            ShopType = shopType;
            StartsAt = startsAt;
            EndsAt = endsAt;
            IsActive = isActive;
            SortOrder = sortOrder;
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = DateTimeOffset.UtcNow;

            EnforceInvariants();
        }

        public void Update(
            string name,
            ShopType shopType,
            DateTimeOffset? startsAt,
            DateTimeOffset? endsAt,
            bool isActive,
            int sortOrder)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));

            Name = name.Trim();
            ShopType = shopType;
            StartsAt = startsAt;
            EndsAt = endsAt;
            IsActive = isActive;
            SortOrder = sortOrder;
            Touch();
            EnforceInvariants();
        }

        public void Activate() { IsActive = true; Touch(); }
        public void Deactivate() { IsActive = false; Touch(); }

        public bool IsOpenAt(DateTimeOffset now)
        {
            if (!IsActive) return false;
            if (ShopType == ShopType.General) return true;
            return StartsAt.HasValue && EndsAt.HasValue
                && now >= StartsAt.Value && now <= EndsAt.Value;
        }

        private void EnforceInvariants()
        {
            if (ShopType == ShopType.TimeLimited)
            {
                if (!StartsAt.HasValue || !EndsAt.HasValue)
                    throw new InvalidOperationException("TimeLimited shop must have StartsAt and EndsAt.");
                if (EndsAt.Value <= StartsAt.Value)
                    throw new InvalidOperationException("EndsAt must be after StartsAt.");
            }
        }

        private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
    }
}
