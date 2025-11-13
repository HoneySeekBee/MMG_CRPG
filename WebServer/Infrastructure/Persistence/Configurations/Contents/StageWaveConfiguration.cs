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
    public class StageWaveConfiguration : IEntityTypeConfiguration<StageWave>
    {
        public void Configure(EntityTypeBuilder<StageWave> e) 
        {
            e.ToTable("StageWaves");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();

            e.Property(x => x.StageId).HasColumnName("stage_id").IsRequired();
            e.Property(x => x.Index).HasColumnName("index").HasColumnType("smallint").IsRequired();
            e.Property(x => x.BatchNum).HasColumnName("batch_num").IsRequired();
            e.HasIndex(x => new { x.StageId, x.Index }).IsUnique(); // 웨이브 순번 중복 방지

            e.HasMany(x => x.Enemies)
             .WithOne()
             .HasForeignKey(en => en.StageWaveId)
             .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
