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
    internal class IconConfiguration : IEntityTypeConfiguration<Icon>
    {
        public void Configure(EntityTypeBuilder<Icon> e)
        {
            e.ToTable("Icons"); // 테이블 명
            e.HasKey(x => x.IconId);
            e.Property(x => x.Key).IsRequired();
            e.HasIndex(x => x.Key).IsUnique();
        }

    }
}
