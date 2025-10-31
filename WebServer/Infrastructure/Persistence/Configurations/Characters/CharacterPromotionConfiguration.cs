using Domain.Entities.Characters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Configurations.Characters
{
    internal class CharacterPromotionConfiguration : IEntityTypeConfiguration<CharacterPromotion>, IEntityTypeConfiguration<CharacterPromotionMaterial>
    {
        void IEntityTypeConfiguration<CharacterPromotionMaterial>.Configure(EntityTypeBuilder<CharacterPromotionMaterial> e)
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


        public void Configure(EntityTypeBuilder<CharacterPromotion> e)
        {
            e.ToTable("CharacterPromotion");

            e.HasKey(x => new { x.CharacterId, x.Tier });

            e.Property(x => x.MaxLevel).IsRequired();
            e.Property(x => x.CostGold).IsRequired();

            e.Property(x => x.Bonus)
                .HasColumnType("jsonb")
                .IsRequired(false);

            e.HasOne(x => x.Character)
             .WithMany(c => c.CharacterPromotions)
             .HasForeignKey(x => x.CharacterId)
             .HasPrincipalKey(c => c.Id)
             .OnDelete(DeleteBehavior.Cascade);

            e.ToTable(t =>
            {
                t.HasCheckConstraint("ck_cp_tier", "\"Tier\" >= 0");
                t.HasCheckConstraint("ck_cp_maxlevel", "\"MaxLevel\" >= 1");
                t.HasCheckConstraint("ck_cp_gold", "\"CostGold\" >= 0");
            });

            e.HasIndex(x => x.CharacterId);
        }

    }
}
