using Domain.Entities.Shop;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Shop
{
    public class ShopConfiguration : IEntityTypeConfiguration<Domain.Entities.Shop.Shop>
    {
        public void Configure(EntityTypeBuilder<Domain.Entities.Shop.Shop> e)
        {
            e.ToTable("Shops");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();

            e.Property(x => x.Code).IsRequired().HasMaxLength(100);
            e.HasIndex(x => x.Code).IsUnique();

            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.Property(x => x.ShopType).HasConversion<short>().IsRequired();
            e.Property(x => x.StartsAt).HasColumnType("datetime(6)");
            e.Property(x => x.EndsAt).HasColumnType("datetime(6)");
            e.Property(x => x.IsActive).IsRequired();
            e.Property(x => x.SortOrder).HasDefaultValue(0);
            e.Property(x => x.CreatedAt).HasColumnType("datetime(6)").IsRequired();
            e.Property(x => x.UpdatedAt).HasColumnType("datetime(6)").IsRequired();

            e.HasMany(x => x.Products)
             .WithOne()
             .HasForeignKey(x => x.ShopId)
             .OnDelete(DeleteBehavior.Cascade);

            var nav = e.Metadata.FindNavigation(nameof(Domain.Entities.Shop.Shop.Products));
            nav!.SetField("_products");
            nav.SetPropertyAccessMode(PropertyAccessMode.Field);

            e.HasIndex(x => x.IsActive);
            e.HasIndex(x => x.ShopType);
        }
    }
}
