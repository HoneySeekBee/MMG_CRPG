using Application.Combat;
using Application.Repositories;
using Domain.Entities;
using Domain.Events;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public sealed class EfCombatRepository : ICombatRepository
    {
        private readonly GameDBContext _db;
        private static readonly JsonSerializerOptions JsonOpt = new(JsonSerializerDefaults.Web);

        public EfCombatRepository(GameDBContext db) => _db = db;
        public async Task<long> SaveAsync(Combat combat,
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
                await _db.SaveChangesAsync(ct);   // Id 생성

                combat.SetId(c.Id);

                // 2) Logs bulk insert
                var logs = events.Select(e => new CombatLogRecord
                {
                    CombatId = c.Id,
                    TMs = e.TMs,
                    PayloadJson = JsonSerializer.Serialize(e) // or 필요한 필드만
                });

                await _db.CombatLogs.AddRangeAsync(logs, ct);
                await _db.SaveChangesAsync(ct);

                await tx.CommitAsync(ct);
                return c.Id;
            });
        }
        public async Task<CombatLogPageDto> GetLogAsync(long combatId, string? cursor, int size, CancellationToken ct)
        {
            // 1) 기본 쿼리
            var q = _db.CombatLogs
                .AsNoTracking()
                .Where(x => x.CombatId == combatId);

            // 2) 커서 파싱 (t_ms 및 id 기준)
            int lastT = 0; long lastId = 0;
            if (!string.IsNullOrWhiteSpace(cursor))
            {
                var parts = cursor.Split('_', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (parts.Length == 2 && int.TryParse(parts[0], out var t) && long.TryParse(parts[1], out var id))
                {
                    lastT = t; lastId = id;
                    q = q.Where(x => x.TMs > lastT || (x.TMs == lastT && x.Id > lastId));
                }
                // 잘못된 커서는 무시(예외 대신 전체 첫 페이지 반환)
            }

            // 3) 정렬 + 페이지
            var rows = await q
                .OrderBy(x => x.TMs).ThenBy(x => x.Id)
                .Take(size + 1) // 다음 커서 탐지용 1개 더
                .Select(x => new
                {
                    x.Id,
                    x.TMs,
                    x.PayloadJson
                })
                .ToListAsync(ct);

            // 4) DTO 변환
            var events = rows.Take(Math.Min(size, rows.Count))
                .Select(r =>
                {
                    // 저장 형태에 맞게 역직렬화/매핑
                    var e = JsonSerializer.Deserialize<Domain.Events.CombatLogEvent>(r.PayloadJson)
                             ?? new Domain.Events.CombatLogEvent(r.TMs, "unknown", null, null, null, null, null);
                    return new CombatLogEventDto(e.TMs, e.Type, e.Actor, e.Target, e.Damage, e.Crit, e.Extra);
                })
                .ToList();

            // 5) nextCursor 구성
            string? nextCursor = null;
            if (rows.Count > size)
            {
                var last = rows[size - 1];
                nextCursor = $"{last.TMs}_{last.Id}";
            }

            return new CombatLogPageDto(combatId, events, nextCursor);
        }

        public async Task<CombatLogSummaryDto> GetSummaryAsync(long combatId, CancellationToken ct)
        {
            // 간단 합계(필요시 LINQ to SQL로)
            var total = await _db.CombatLogs.AsNoTracking().Where(x => x.CombatId == combatId).CountAsync(ct);
            var duration = await _db.CombatLogs.AsNoTracking().Where(x => x.CombatId == combatId).MaxAsync(x => (int?)x.TMs, ct) ?? 0;

            // 피해 합계는 JSON 파싱이 필요하므로 메모리 내 계산(로그량 크면 뷰/함수 고려)
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

        private static (int? t, long? id) ParseCursor(string? cursor)
        {
            if (string.IsNullOrWhiteSpace(cursor)) return (null, null);
            var parts = cursor.Split(',');
            int? t = null; long? id = null;
            foreach (var p in parts)
            {
                if (p.StartsWith("t:")) t = int.Parse(p.AsSpan(2));
                if (p.StartsWith("id:")) id = long.Parse(p.AsSpan(3));
            }
            return (t, id);
        }
    }
}