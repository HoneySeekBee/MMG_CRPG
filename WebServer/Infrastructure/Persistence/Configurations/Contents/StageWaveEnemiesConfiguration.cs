using Domain.Entities.Contents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Configurations.Contents
{
    public class StageWaveEnemiesConfiguration : IEntityTypeConfiguration<StageWaveEnemy>
    {
        public void Configure(EntityTypeBuilder<StageWaveEnemy> e) 
        {
            e.ToTable("StageWaveEnemies");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();

            e.Property(x => x.StageWaveId).HasColumnName("stage_wave_id").IsRequired();
            e.Property(x => x.EnemyCharacterId).HasColumnName("enemy_character_id").IsRequired();
            e.Property(x => x.Level).HasColumnName("level").HasColumnType("smallint").IsRequired();
            e.Property(x => x.Slot).HasColumnName("slot").HasColumnType("smallint").IsRequired();
            e.Property(x => x.AiProfile).HasColumnName("ai_profile").IsRequired(false);

            e.HasIndex(x => x.StageWaveId);
            e.HasIndex(x => new { x.StageWaveId, x.Slot }).IsUnique(); // 슬롯 중복 방지

            // FK는 상단 StageWave에서 Cascade 설정 이미 수행됨
        }
    }
}
