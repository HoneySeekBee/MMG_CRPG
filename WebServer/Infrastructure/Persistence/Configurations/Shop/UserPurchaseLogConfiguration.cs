using Domain.Entities.Shop;
using Domain.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Shop
{
    public class UserPurchaseLogConfiguration : IEntityTypeConfiguration<UserPurchaseLog>
    {
        public void Configure(EntityTypeBuilder<UserPurchaseLog> e)
        {
            e.ToTable("UserPurchaseLogs");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();

            e.Property(x => x.UserId).IsRequired();
            e.Property(x => x.ShopProductId).IsRequired();
            e.Property(x => x.Quantity).IsRequired();
            e.Property(x => x.PricePaid).IsRequired();
            e.Property(x => x.CurrencyCode).IsRequired().HasMaxLength(50);
            e.Property(x => x.PurchasedAt).HasColumnType("timestamptz").IsRequired();

            e.HasOne<User>().WithMany().HasForeignKey(x => x.UserId);
            e.HasOne<ShopProduct>().WithMany().HasForeignKey(x => x.ShopProductId);

            e.HasIndex(x => new { x.UserId, x.ShopProductId, x.PurchasedAt })
             .HasDatabaseName("ix_purchase_log_user_product_date");
            e.HasIndex(x => x.PurchasedAt);
        }
    }
}
