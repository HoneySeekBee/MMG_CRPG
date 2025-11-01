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
    public class CharacterModelWeaponConfiguration : IEntityTypeConfiguration<CharacterModelWeapon>
    {
        public void Configure(EntityTypeBuilder<CharacterModelWeapon> e)
        {
            e.ToTable("CharacterModelWeapon");
            e.HasKey(x => x.WeaponId);

            e.Property(x => x.WeaponId).HasColumnName("weapon_id");
            e.Property(x => x.Code).HasColumnName("code").IsRequired();
            e.Property(x => x.DisplayName).HasColumnName("display_name").IsRequired();
            e.Property(x => x.IsTwoHanded).HasColumnName("is_two_handed").IsRequired();

            e.HasIndex(x => x.Code).IsUnique().HasDatabaseName("ux_weapon_code");
        }
    }
}
