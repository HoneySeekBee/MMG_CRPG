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
    internal class CharacterStatProgressionConfiguration : IEntityTypeConfiguration<CharacterStatProgression>
    {
        public void Configure(EntityTypeBuilder<CharacterStatProgression> e)
        {
            e.ToTable("CharacterStatProgression");

            e.HasKey(x => new { x.CharacterId, x.Level });

            e.Property(x => x.CharacterId).HasColumnName("CharacterId").IsRequired();
            e.Property(x => x.Level).HasColumnName("Level").IsRequired();

            e.Property(x => x.HP).IsRequired();
            e.Property(x => x.ATK).IsRequired();
            e.Property(x => x.DEF).IsRequired();
            e.Property(x => x.SPD).IsRequired();
            e.Property(x => x.CriRate).HasPrecision(5, 2).HasDefaultValue(5m).IsRequired();
            e.Property(x => x.CriDamage).HasPrecision(6, 2).HasDefaultValue(150m).IsRequired();
            e.Property(x => x.Range).HasColumnName("Range").IsRequired();

            e.HasOne(x => x.Character)
             .WithMany(c => c.CharacterStatProgressions)
             .HasForeignKey(x => x.CharacterId)
             .HasPrincipalKey(c => c.Id)
             .OnDelete(DeleteBehavior.Cascade);

            e.ToTable(t =>
            {
                t.HasCheckConstraint("ck_csp_level", "\"Level\" >= 1");
                t.HasCheckConstraint("ck_csp_stats", "\"HP\" >= 0 AND \"ATK\" >= 0 AND \"DEF\" >= 0 AND \"SPD\" >= 0");
                t.HasCheckConstraint("ck_csp_cr", "\"CriRate\" >= 0 AND \"CriRate\" <= 100");
                t.HasCheckConstraint("ck_csp_cd", "\"CriDamage\" >= 0 AND \"CriDamage\" <= 1000");
            });

            e.HasIndex(x => x.CharacterId);
        }
    }
}
