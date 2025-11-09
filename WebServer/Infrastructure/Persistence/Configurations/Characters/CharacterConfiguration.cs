using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Entities.Characters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Persistence.Configurations.Characters
{
    internal class CharacterConfiguration : IEntityTypeConfiguration<Character>
    {
        public void Configure(EntityTypeBuilder<Character> e)
        {
            e.ToTable("Characters");

            e.HasKey(x => x.Id);
            e.Property(x => x.Id)
                .HasColumnName("CharacterId")
                .ValueGeneratedOnAdd();

            // 기본
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.Property(x => x.RarityId).IsRequired();
            e.Property(x => x.FactionId).IsRequired();
            e.Property(x => x.RoleId).IsRequired();
            e.Property(x => x.ElementId).IsRequired();

            // 선택
            e.Property(x => x.IconId).IsRequired(false);
            e.Property(x => x.PortraitId).IsRequired(false);

            e.Property(x => x.FormationNumber).HasColumnName("formation_position");
            var utcConverter = new ValueConverter<DateTimeOffset?, DateTimeOffset?>(
                v => v.HasValue ? v.Value.ToUniversalTime() : v,  // Save: UTC로
                v => v                                            // Read: 그대로(이미 UTC)
            );
            e.Property(x => x.ReleaseDate)
   .HasConversion(utcConverter)
   .IsRequired(false);
            e.Property(x => x.IsLimited).IsRequired().HasDefaultValue(false);

            // Tags: IReadOnlyList<string> → 백필드 _tags 를 text[]로 매핑
            e.Property<List<string>>("_tags")
                .HasColumnName("Tags")
                .HasColumnType("text[]")
                .HasDefaultValueSql("'{}'::text[]")
                .IsRequired();

            // Meta: JSON 문자열을 jsonb 로 저장
            e.Property(x => x.MetaJson)
                .HasColumnName("Meta")
                .HasColumnType("jsonb")
                .IsRequired(false);

            // 인덱스
            e.HasIndex(x => x.Name);
            e.HasIndex(x => x.ElementId);
            e.HasIndex(x => x.RarityId);
            e.HasIndex(x => x.RoleId);
            e.HasIndex(x => x.FactionId);
            e.HasIndex(x => x.IsLimited);
        }



    }
}
