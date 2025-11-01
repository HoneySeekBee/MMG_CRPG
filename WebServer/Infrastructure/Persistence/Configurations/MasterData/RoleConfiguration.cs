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
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> e) 
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
    }
}
