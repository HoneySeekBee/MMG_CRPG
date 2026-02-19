using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace Infrastructure.Persistence.Configurations.Items
{
    public class ItemEffectConfiguration :IEntityTypeConfiguration<ItemEffect>
    {
        public void Configure(EntityTypeBuilder<ItemEffect> e)
        {
            e.ToTable("ItemEffect");
            e.HasKey(x => x.Id);

            e.Property(x => x.Payload).HasColumnType("json")
                .HasConversion(
                    v => DocToString(v),
                    v => StringToDoc(v));
            e.Property(x => x.SortOrder).HasDefaultValue((short)0);
        }

        private static string? DocToString(JsonDocument? doc)
            => doc is null ? null : doc.RootElement.GetRawText();
        private static JsonDocument? StringToDoc(string? s)
            => string.IsNullOrWhiteSpace(s) ? null : JsonDocument.Parse(s);
    }
}
