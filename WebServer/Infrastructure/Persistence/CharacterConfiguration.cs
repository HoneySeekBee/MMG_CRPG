using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Persistence
{
    internal class CharacterConfiguration : IEntityTypeConfiguration<Character>,
        IEntityTypeConfiguration<CharacterStatProgression>,
         IEntityTypeConfiguration<CharacterPromotionMaterial>,
         IEntityTypeConfiguration<CharacterPromotion>,
         IEntityTypeConfiguration<CharacterSkill>
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

        void IEntityTypeConfiguration<CharacterStatProgression>.Configure(EntityTypeBuilder<CharacterStatProgression> e)
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


        void IEntityTypeConfiguration<CharacterPromotion>.Configure(EntityTypeBuilder<CharacterPromotion> e)
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

        void IEntityTypeConfiguration<CharacterSkill>.Configure(EntityTypeBuilder<CharacterSkill> e)
        {
            e.ToTable("CharacterSkills");

            e.HasKey(x => new { x.CharacterId, x.Slot });

            e.Property(x => x.CharacterId).HasColumnName("CharacterId").IsRequired();
            e.Property(x => x.Slot).HasConversion<short>().IsRequired();
            e.Property(x => x.SkillId).IsRequired();
            e.Property(x => x.UnlockTier).HasDefaultValue(0);
            e.Property(x => x.UnlockLevel).HasDefaultValue(0);

            // (선택) 캐릭터 내 동일 스킬 중복 방지
            // e.HasAlternateKey(x => new { x.CharacterId, x.SkillId });

            e.HasOne(x => x.Character)
             .WithMany(c => c.CharacterSkills)            // ← Character 엔티티에 컬렉션 네비게이션 선언 필요
             .HasForeignKey(x => x.CharacterId)
             .HasPrincipalKey(c => c.Id)                  // ← 주키를 명확히
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne<Skill>()
             .WithMany()
             .HasForeignKey(x => x.SkillId)
             .OnDelete(DeleteBehavior.Restrict);

            e.ToTable(t =>
            {
                t.HasCheckConstraint("ck_cs_unlock_tier", "\"UnlockTier\" >= 0");
                t.HasCheckConstraint("ck_cs_unlock_level", "\"UnlockLevel\" >= 1");
                t.HasCheckConstraint("ck_cs_slot", "\"Slot\" BETWEEN 1 AND 4");
            });

            e.HasIndex(x => x.SkillId);
        }
    }
}
