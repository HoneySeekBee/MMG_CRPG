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
    public class CharacterModelConfiguration : IEntityTypeConfiguration<CharacterModel>  
    {
        public void Configure(EntityTypeBuilder<CharacterModel> e)
        {
            e.ToTable("CharacterModel");
            e.HasKey(x => x.CharacterId);

            e.Property(x => x.CharacterId).HasColumnName("character_id");
            e.Property(x => x.BodyType)
           .HasColumnName("body_type")
           .HasColumnType("BodySize");

            e.Property(x => x.AnimationType)
                .HasColumnName("animation_type")
                .HasColumnType("CharacterAnimationType");
            e.Property(x => x.WeaponLId).HasColumnName("weapon_l_id").IsRequired(false);
            e.Property(x => x.WeaponRId).HasColumnName("weapon_r_id").IsRequired(false);

            e.Property(x => x.PartHeadId).HasColumnName("part_head_id").IsRequired(false);
            e.Property(x => x.PartHairId).HasColumnName("part_hair_id").IsRequired(false);
            e.Property(x => x.PartMouthId).HasColumnName("part_mouth_id").IsRequired(false);
            e.Property(x => x.PartEyeId).HasColumnName("part_eye_id").IsRequired(false);
            e.Property(x => x.PartAccId).HasColumnName("part_acc_id").IsRequired(false);
            e.Property(x => x.HairColorCode).HasColumnName("hair_color_code").IsRequired(false);
            e.Property(x => x.SkinColorCode).HasColumnName("skin_color_code").IsRequired(false);

            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamptz").HasDefaultValueSql("now()");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamptz").HasDefaultValueSql("now()");

            // ─── Foreign Keys ─────────────────────────────────────────────────

            // 무기 FK (삭제 시 NULL)
            e.HasOne<CharacterModelWeapon>()
                .WithMany()
                .HasForeignKey(x => x.WeaponLId)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasOne<CharacterModelWeapon>()
                .WithMany()
                .HasForeignKey(x => x.WeaponRId)
                .OnDelete(DeleteBehavior.SetNull);

            // 파츠 FK (삭제 시 NULL)
            e.HasOne<CharacterModelPart>().WithMany().HasForeignKey(x => x.PartHeadId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne<CharacterModelPart>().WithMany().HasForeignKey(x => x.PartHairId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne<CharacterModelPart>().WithMany().HasForeignKey(x => x.PartMouthId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne<CharacterModelPart>().WithMany().HasForeignKey(x => x.PartEyeId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne<CharacterModelPart>().WithMany().HasForeignKey(x => x.PartAccId).OnDelete(DeleteBehavior.SetNull);

            // 인덱스
            e.HasIndex(x => x.BodyType).HasDatabaseName("ix_cm_body_type");
            e.HasIndex(x => x.AnimationType).HasDatabaseName("ix_cm_anim_type");
            e.HasIndex(x => x.WeaponLId).HasDatabaseName("ix_cm_weapon_l");
            e.HasIndex(x => x.WeaponRId).HasDatabaseName("ix_cm_weapon_r");
        }
    }
}
