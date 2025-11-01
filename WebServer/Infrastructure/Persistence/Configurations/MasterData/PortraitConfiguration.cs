using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Configurations.MasterData
{
    public class PortraitConfiguration : IEntityTypeConfiguration<Portrait>
    {
        public void Configure(EntityTypeBuilder<Portrait> e)
        {
            e.ToTable("Portraits");                 // 테이블명
            e.HasKey(x => x.PortraitId);
            e.Property(x => x.PortraitId).ValueGeneratedOnAdd();

            e.Property(x => x.Key).IsRequired();    // 파일 키(유니크 권장)
            e.HasIndex(x => x.Key).IsUnique();

            // 스프라이트 좌표/아틀라스(없으면 NULL 허용)
            e.Property(x => x.Atlas).IsRequired(false);
            e.Property(x => x.X).IsRequired(false);
            e.Property(x => x.Y).IsRequired(false);
            e.Property(x => x.W).IsRequired(false);
            e.Property(x => x.H).IsRequired(false);

            // 캐시 무효화를 위한 버전
            e.Property(x => x.Version).HasDefaultValue(1);
        }

    }
}
