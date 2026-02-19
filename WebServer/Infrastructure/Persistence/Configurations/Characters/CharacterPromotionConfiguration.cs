using Domain.Entities.Characters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace Infrastructure.Persistence.Configurations.Characters
{
    public class CharacterPromotionConfiguration : IEntityTypeConfiguration<CharacterPromotion>
    {
        public void Configure(EntityTypeBuilder<CharacterPromotion> e)
        {
            e.ToTable("CharacterPromotion");

            e.HasKey(x => new { x.CharacterId, x.Tier });

            e.Property(x => x.MaxLevel).IsRequired();
            e.Property(x => x.CostGold).IsRequired();

            e.Property(x => x.Bonus)
                .HasColumnType("json")
                .HasConversion(
                    v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => string.IsNullOrEmpty(v) ? null : JsonSerializer.Deserialize<StatModifier>(v, (JsonSerializerOptions?)null))
                .IsRequired(false);

            e.HasOne(x => x.Character)
             .WithMany(c => c.CharacterPromotions)
             .HasForeignKey(x => x.CharacterId)
             .HasPrincipalKey(c => c.Id)
             .OnDelete(DeleteBehavior.Cascade);

            e.ToTable(t =>
            {
                t.HasCheckConstraint("ck_cp_tier", "`Tier` >= 0");
                t.HasCheckConstraint("ck_cp_maxlevel", "`MaxLevel` >= 1");
                t.HasCheckConstraint("ck_cp_gold", "`CostGold` >= 0");
            });

            e.HasIndex(x => x.CharacterId);
        }

    }
}
