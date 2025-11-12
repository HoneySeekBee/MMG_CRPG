using Domain.Entities.Contents;
using Domain.Entities.Monsters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Configurations.Monsters
{
    public class MonsterConfiguration : IEntityTypeConfiguration<Monster>
    {
        public void Configure(EntityTypeBuilder<Monster> b)
        {
            b.ToTable("Monsters");
            b.HasKey(m => m.Id);
            b.Property(m => m.Id).HasColumnName("monster_id");
            b.Property(m => m.Name).HasColumnName("name");
            b.Property(m => m.ModelKey).HasColumnName("model_key");
            b.Property(m => m.ElementId).HasColumnName("element_id");
            b.Property(m => m.PortraitId).HasColumnName("portrait_id");

            b.HasMany(m => m.Stats)
             .WithOne(s => s.Monster)
             .HasForeignKey(s => s.MonsterId);
        }
    }
}
