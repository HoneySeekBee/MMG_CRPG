using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Application.Factions
{
    public sealed class FactionDto
    {
        public int FactionId { get; init; }
        public string Key { get; init; } = "";
        public string Label { get; init; } = "";
        public int? IconId { get; init; }
        public string? ColorHex { get; init; }
        public short SortOrder { get; init; }
        public bool IsActive { get; init; }
        public string? Meta { get; init; }

        public static FactionDto From(Faction e)
        {

            string? description = null;

            if (!string.IsNullOrWhiteSpace(e.Meta))
            {
                try
                {
                    using var doc = JsonDocument.Parse(e.Meta);
                    description = doc.RootElement.GetProperty("description").GetString();
                }
                catch
                {

                }
            }

            return new FactionDto
            {
                FactionId = e.FactionId,
                Key = e.Key,
                Label = e.Label,
                ColorHex = e.ColorHex,
                IconId = e.IconId,
                SortOrder = e.SortOrder,
                IsActive = e.IsActive,
                Meta = description
            };
        }
    }
}
