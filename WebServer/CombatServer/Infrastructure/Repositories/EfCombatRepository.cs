using Application.Combat;
using Application.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Infrastructure.Repositories
{
    public sealed class EfCombatRepository : ICombatRepository
    {
        private readonly CombatDbContext _db;
        private static readonly JsonSerializerOptions JsonOpt = new(JsonSerializerDefaults.Web);

        public EfCombatRepository(CombatDbContext db) => _db = db;

        public async Task<long> SaveAsync(Domain.Entities.Combats.Combat combat,
            IEnumerable<Domain.Events.CombatLogEvent> events,
            CancellationToken ct)
        {
            var strategy = _db.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _db.Database.BeginTransactionAsync(ct);

                // 1) Combat insert
                var c = new CombatRecord
                {
                    Mode = combat.Mode.ToString(),
                    Seed = combat.Seed,
                    StageId = combat.StageId,
                    InputJson = JsonSerializer.Serialize(combat.Input),
                    Result = combat.Result.ToString(),
                    ClearMs = combat.ClearMs,
                    BalanceVersion = combat.BalanceVersion,
                    ClientVersion = combat.ClientVersion,
                    CreatedAt = combat.CreatedAt.UtcDateTime
                };
                _db.Combats.Add(c);
                await _db.SaveChangesAsync(ct);   // Generate Id

                combat.SetId(c.Id);

                // 2) Logs bulk insert
                var logs = events.Select(e => new CombatLogRecord
                {
                    CombatId = c.Id,
                    TMs = e.TMs,
                    PayloadJson = JsonSerializer.Serialize(e)
                });

                await _db.CombatLogs.AddRangeAsync(logs, ct);
                await _db.SaveChangesAsync(ct);

                await tx.CommitAsync(ct);
                return c.Id;
            });
        }

        public async Task AppendLogsAsync(long combatId, IEnumerable<Domain.Events.CombatLogEvent> events, CancellationToken ct)
        {
            var logs = events.Select(e => new CombatLogRecord
            {
                CombatId = combatId,
                TMs = e.TMs,
                PayloadJson = JsonSerializer.Serialize(e)
            });

            await _db.CombatLogs.AddRangeAsync(logs, ct);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<CombatLogPageDto> GetLogAsync(long combatId, string? cursor, int size, CancellationToken ct)
        {
            // 1) Base query
            var q = _db.CombatLogs
                .AsNoTracking()
                .Where(x => x.CombatId == combatId);

            // 2) Cursor parsing (by t_ms and id)
            int lastT = 0; long lastId = 0;
            if (!string.IsNullOrWhiteSpace(cursor))
            {
                var parts = cursor.Split('_', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (parts.Length == 2 && int.TryParse(parts[0], out var t) && long.TryParse(parts[1], out var id))
                {
                    lastT = t; lastId = id;
                    q = q.Where(x => x.TMs > lastT || (x.TMs == lastT && x.Id > lastId));
                }
            }

            // 3) Sort + page
            var rows = await q
                .OrderBy(x => x.TMs).ThenBy(x => x.Id)
                .Take(size + 1)
                .Select(x => new
                {
                    x.Id,
                    x.TMs,
                    x.PayloadJson
                })
                .ToListAsync(ct);

            // 4) DTO mapping
            var eventsResult = rows.Take(Math.Min(size, rows.Count))
                .Select(r =>
                {
                    var e = JsonSerializer.Deserialize<Domain.Events.CombatLogEvent>(r.PayloadJson)
                             ?? new Domain.Events.CombatLogEvent(r.TMs, "unknown", null, null, null, null, null);
                    return new CombatLogEventDto(e.TMs, e.Type, e.Actor, e.Target, e.Damage, e.Crit, e.Extra);
                })
                .ToList();

            // 5) Build nextCursor
            string? nextCursor = null;
            if (rows.Count > size)
            {
                var last = rows[size - 1];
                nextCursor = $"{last.TMs}_{last.Id}";
            }

            return new CombatLogPageDto(combatId, eventsResult, nextCursor);
        }

        public async Task<CombatLogSummaryDto> GetSummaryAsync(long combatId, CancellationToken ct)
        {
            var total = await _db.CombatLogs.AsNoTracking().Where(x => x.CombatId == combatId).CountAsync(ct);
            var duration = await _db.CombatLogs.AsNoTracking().Where(x => x.CombatId == combatId).MaxAsync(x => (int?)x.TMs, ct) ?? 0;

            var chunk = await _db.CombatLogs.AsNoTracking()
                .Where(x => x.CombatId == combatId)
                .Select(x => x.PayloadJson)
                .ToListAsync(ct);

            int dmg = 0;
            foreach (var s in chunk)
            {
                using var doc = JsonDocument.Parse(s);
                if (doc.RootElement.TryGetProperty("dmg", out var dEl) && dEl.ValueKind == JsonValueKind.Number)
                    dmg += dEl.GetInt32();
            }

            return new CombatLogSummaryDto(combatId, total, duration, dmg, 0);
        }
    }
}
