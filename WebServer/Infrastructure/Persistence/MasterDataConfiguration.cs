using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence
{
    internal class MasterDataConfiguration : IEntityTypeConfiguration<Icon>,
        IEntityTypeConfiguration<Portrait>,
        IEntityTypeConfiguration<Element>,
        IEntityTypeConfiguration<ElementAffinity>,
        IEntityTypeConfiguration<Faction>,
        IEntityTypeConfiguration<Role>,
        IEntityTypeConfiguration<Rarity>
    {
        public void Configure(EntityTypeBuilder<Icon> e)
        {
            e.ToTable("Icons"); // 테이블 명
            e.HasKey(x => x.IconId);
            e.Property(x => x.Key).IsRequired();
            e.HasIndex(x => x.Key).IsUnique();
        }
        void IEntityTypeConfiguration<Portrait>.Configure(EntityTypeBuilder<Portrait> e)
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
        void IEntityTypeConfiguration<Element>.Configure(EntityTypeBuilder<Element> e)
        {
            e.ToTable("Element");
            e.HasKey(x => x.ElementId);
            e.Property(x => x.Key).IsRequired();
            e.Property(x => x.Label).IsRequired();
            e.Property(x => x.ColorHex).IsRequired();
            e.Property(x => x.SortOrder).HasColumnType("smallint");
            e.Property(x => x.Meta).HasColumnType("jsonb"); // PostgreSQL
            e.HasIndex(x => new { x.IsActive, x.SortOrder });
            e.HasIndex(x => x.Key).IsUnique();
        }
        void IEntityTypeConfiguration<ElementAffinity>.Configure(EntityTypeBuilder<ElementAffinity> e)
        {
            e.ToTable("ElementAffinity");
            e.HasKey(x => new { x.AttackerElementId, x.DefenderElementId });
            e.Property(x => x.Multiplier)
            .HasColumnType("numeric(4,2)")
            .HasDefaultValue(1.00m);
        }
        void IEntityTypeConfiguration<Faction>.Configure(EntityTypeBuilder<Faction> e)
        {
            e.ToTable("Faction");
            e.HasKey(x => x.FactionId);
            e.Property(x => x.FactionId).ValueGeneratedOnAdd();
            e.Property(x => x.Key).IsRequired();
            e.Property(x => x.Label).IsRequired();
            e.Property(x => x.ColorHex);
            e.Property(x => x.Meta).HasColumnType("jsonb");      // pg jsonb
            e.Property(x => x.IsActive).HasDefaultValue(true);

            e.HasIndex(x => x.Key).IsUnique();
        }
        void IEntityTypeConfiguration<Role>.Configure(EntityTypeBuilder<Role> e)
        {
            e.ToTable("Role");
            e.HasKey(x => x.RoleId);
            e.Property(x => x.RoleId).ValueGeneratedOnAdd();
            e.Property(x => x.Key).IsRequired();
            e.Property(x => x.Label).IsRequired();
            e.Property(x => x.ColorHex);
            e.Property(x => x.Meta).HasColumnType("jsonb");
            e.Property(x => x.IsActive).HasDefaultValue(true);

            e.HasIndex(x => x.Key).IsUnique();
        }
        void IEntityTypeConfiguration<Rarity>.Configure(EntityTypeBuilder<Rarity> e)
        {
            e.ToTable("Rarity");
            e.HasKey(x => x.RarityId);
            e.Property(x => x.RarityId).ValueGeneratedOnAdd();
            e.Property(x => x.Stars).IsRequired();
            e.Property(x => x.Key).IsRequired();
            e.Property(x => x.Label).IsRequired();
            e.Property(x => x.ColorHex);
            e.Property(x => x.Meta).HasColumnType("jsonb");
            e.Property(x => x.IsActive).HasDefaultValue(true);

            e.HasIndex(x => x.Key).IsUnique();
        }
    }
}
