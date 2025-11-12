using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Configurations.Items
{
    public class ItemEffectConfiguration :IEntityTypeConfiguration<ItemEffect>
    {
        public void Configure(EntityTypeBuilder<ItemEffect> e) 
        {
            e.ToTable("ItemEffect");
            e.HasKey(x => x.Id);

            e.Property(x => x.Payload).HasColumnType("jsonb");
            e.Property(x => x.SortOrder).HasDefaultValue((short)0);
        }

    }
}
