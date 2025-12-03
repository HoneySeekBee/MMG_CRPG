using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Configurations.Items
{
    public class ItemConfiguration : IEntityTypeConfiguration<Item>  
    {
        public void Configure(EntityTypeBuilder<Item> e)
        {
            e.ToTable("Item");
            e.HasKey(x => x.Id);

            e.Property(x => x.Code).IsRequired();
            e.HasIndex(x => x.Code).IsUnique();

            e.Property(x => x.Name).IsRequired();
            e.Property(x => x.Description).HasDefaultValue("");

            // FK 값들: (참조 테이블은 다른 모델에서 구성되어 있다고 가정)
            e.Property(x => x.TypeId).IsRequired();
            e.Property(x => x.RarityId).IsRequired();

            e.Property(x => x.Stackable).HasDefaultValue(true);
            e.Property(x => x.MaxStack).HasDefaultValue(99);
            e.Property(x => x.Tradable).HasDefaultValue(true);
            e.Property(x => x.Weight).HasDefaultValue(0);

            // string[] -> text[] (Npgsql이 자동 매핑)
            e.Property(x => x.Tags)
    .HasColumnType("jsonb")
    .HasConversion(
        v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
        v => JsonSerializer.Deserialize<string[]>(v, (JsonSerializerOptions)null) ?? Array.Empty<string>()
    )
    .HasDefaultValueSql("'[]'::jsonb");

            // JsonNode -> jsonb
            e.Property(x => x.Meta).HasColumnType("jsonb");



            e.Property(x => x.CreatedAt).HasColumnType("timestamptz");
            e.Property(x => x.UpdatedAt).HasColumnType("timestamptz");

            e.Property(x => x.EquipType);


            e.HasMany(i => i.Stats)
             .WithOne()
             .HasForeignKey(x => x.ItemId)
             .OnDelete(DeleteBehavior.Cascade);
            e.Navigation(i => i.Stats).HasField("_stats")
                                       .UsePropertyAccessMode(PropertyAccessMode.Field);


            e.HasMany(i => i.Effects)
.WithOne()
.HasForeignKey(x => x.ItemId)
.OnDelete(DeleteBehavior.Cascade);
            e.Navigation(i => i.Effects).HasField("_effects")
                                         .UsePropertyAccessMode(PropertyAccessMode.Field);


            e.HasMany(i => i.Prices)
             .WithOne()
             .HasForeignKey(x => x.ItemId)
             .OnDelete(DeleteBehavior.Cascade);
            e.Navigation(i => i.Prices).HasField("_prices")
                            .UsePropertyAccessMode(PropertyAccessMode.Field);
        }

    }
}
