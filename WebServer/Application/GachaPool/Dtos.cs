using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.GachaPool
{
    public sealed record GachaPoolDto(
        int PoolId,
        string Name,
        DateTimeOffset ScheduleStart,
        DateTimeOffset? ScheduleEnd,
        string? TablesVersion
    );

    public sealed record GachaPoolEntryDto(
        int CharacterId,
        short Grade,
        bool RateUp,
        int Weight
    );

    public sealed record GachaPoolDetailDto(
        GachaPoolDto Pool,
        string? PityJson,
        string? ConfigJson,
        IReadOnlyList<GachaPoolEntryDto> Entries
    );

    public static class GachaPoolMappings
    {
        public static GachaPoolDto ToDto(this Domain.Entities.GachaPool e)
            => new(e.PoolId, e.Name, e.ScheduleStart, e.ScheduleEnd, e.TablesVersion);

        public static GachaPoolDetailDto ToDetailDto(this Domain.Entities.GachaPool e)
        {
            var pool = e.ToDto();
            var entries = e.Entries.Select(x => new GachaPoolEntryDto(x.CharacterId, x.Grade, x.RateUp, x.Weight)).ToList();
            return new(pool, e.PityJson, e.Config, entries);
        }

        public static Domain.Entities.GachaPoolEntry ToEntity(this GachaPoolEntryDto d)
            => Domain.Entities.GachaPoolEntry.Create(d.CharacterId, d.Grade, d.RateUp, d.Weight);
    }
}
