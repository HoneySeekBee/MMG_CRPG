using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Gacha.GachaPool
{
    public sealed record QueryGachaPoolsRequest(string? Keyword = null, int Skip = 0, int Take = 20);

    public sealed record CreateGachaPoolRequest(
        string Name,
        DateTimeOffset? ScheduleStart,
        DateTimeOffset? ScheduleEnd,
        string? PityJson,
        string? TablesVersion,
        string? ConfigJson
    );

    public sealed record UpdateGachaPoolRequest(
        int PoolId,
        string Name,
        DateTimeOffset ScheduleStart,
        DateTimeOffset? ScheduleEnd,
        string? PityJson,
        string? TablesVersion,
        string? ConfigJson
    );

    public sealed record UpsertGachaPoolEntriesRequest(
        int PoolId,
        IReadOnlyList<GachaPoolEntryDto> Entries
    );
}
