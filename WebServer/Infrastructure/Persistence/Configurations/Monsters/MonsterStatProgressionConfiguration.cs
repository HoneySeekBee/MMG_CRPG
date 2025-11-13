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
    public class MonsterStatProgressionConfiguration : IEntityTypeConfiguration<MonsterStatProgression>
    {
        public void Configure(EntityTypeBuilder<MonsterStatProgression> b)
        {
            b.ToTable("MonsterStatProgression");
            b.HasKey(s => new { s.MonsterId, s.Level });

            b.Property(s => s.MonsterId).HasColumnName("monster_id");
            b.Property(s => s.Level).HasColumnName("level");
            b.Property(s => s.HP).HasColumnName("hp");
            b.Property(s => s.ATK).HasColumnName("atk");
            b.Property(s => s.DEF).HasColumnName("def");
            b.Property(s => s.SPD).HasColumnName("spd");
            b.Property(s => s.CritRate).HasColumnName("cri_rate");
            b.Property(s => s.CritDamage).HasColumnName("cri_damage");
            b.Property(s => s.Range).HasColumnName("range");
        }
    }
}
