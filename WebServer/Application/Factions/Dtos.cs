using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public static FactionDto From(Faction e) => new()
        {
            FactionId = e.FactionId,
            Key = e.Key,
            Label = e.Label,
            IconId = e.IconId,
            ColorHex = e.ColorHex,
            SortOrder = e.SortOrder,
            IsActive = e.IsActive,
            Meta = e.Meta
        };
    }
}
