using Domain.Entities;
using Domain.Entities.Shop;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Shop
{
    public class ShopProductConfiguration : IEntityTypeConfiguration<ShopProduct>
    {
        public void Configure(EntityTypeBuilder<ShopProduct> e)
        {
            e.ToTable("ShopProducts");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();

            e.Property(x => x.ShopId).IsRequired();
            e.Property(x => x.ItemId).IsRequired();
            e.Property(x => x.CurrencyId).IsRequired();
            e.Property(x => x.Price).IsRequired();
            e.Property(x => x.QuantityPerPurchase).HasDefaultValue(1).IsRequired();
            e.Property(x => x.DailyLimit);
            e.Property(x => x.WeeklyLimit);
            e.Property(x => x.TotalLimit);
            e.Property(x => x.SortOrder).HasDefaultValue(0);
            e.Property(x => x.IsActive).IsRequired();
            e.Property(x => x.CreatedAt).HasColumnType("timestamptz").IsRequired();
            e.Property(x => x.UpdatedAt).HasColumnType("timestamptz").IsRequired();

            e.HasOne<Item>().WithMany().HasForeignKey(x => x.ItemId);
            e.HasOne<Currency>().WithMany().HasForeignKey(x => x.CurrencyId);

            e.HasIndex(x => x.ShopId);
            e.HasIndex(x => x.ItemId);
            e.HasIndex(x => x.IsActive);
        }
    }
}
