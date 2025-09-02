using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Rarities
{
    public sealed class RarityDto
    {
        public int RarityId { get; init; }

        public short Stars { get; init; }

        public string Key { get; init; } = "";
        public string Label { get; init; } = "";
        public string? ColorHex { get; init; }
        public short SortOrder { get; init; }
        public bool IsActive { get; init; }
        public string? Meta { get; init; }

        public static RarityDto From(Rarity e) => new()
        {
            RarityId = e.RarityId,
            Stars = e.Stars,       
            Key = e.Key,
            Label = e.Label,
            ColorHex = e.ColorHex,
            SortOrder = e.SortOrder,
            IsActive = e.IsActive,
            Meta = e.Meta
        };
    }
}
