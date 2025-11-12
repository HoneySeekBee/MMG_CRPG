using Domain.Entities.Characters;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Configurations.Characters
{
    public class CharacterPromotionMaterialConfiguration
    : IEntityTypeConfiguration<CharacterPromotionMaterial>
    {
        public void Configure(EntityTypeBuilder<CharacterPromotionMaterial> e)
        {
            e.ToTable("CharacterPromotionMaterials");

            e.Property(x => x.PromotionCharacterId).HasColumnName("CharacterId").IsRequired();
            e.Property(x => x.PromotionTier).HasColumnName("Tier").IsRequired();
            e.Property(x => x.ItemId).IsRequired();
            e.Property(x => x.Count).IsRequired();

            e.HasKey(x => new { x.PromotionCharacterId, x.PromotionTier, x.ItemId });

            e.HasOne(x => x.Promotion)
.WithMany(p => p.Materials)
.HasForeignKey(x => new { x.PromotionCharacterId, x.PromotionTier })
.HasPrincipalKey(p => new { p.CharacterId, p.Tier })
.OnDelete(DeleteBehavior.Cascade);

            // Item FK
            e.HasOne(x => x.Item)
             .WithMany()
             .HasForeignKey(x => x.ItemId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => x.ItemId);
        }
    }
}