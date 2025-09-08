using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    /// <summary>
    ///  가차 풀 : 뽑기 확률 관련 엔티티
    /// </summary>
    public sealed class GachaPool
    {
        // EF용
        private GachaPool() { }

        public int PoolId { get; private set; }              // PK
        public string Name { get; private set; } = default!;  // 표기용

        // 기간
        public DateTimeOffset ScheduleStart { get; private set; }
        public DateTimeOffset? ScheduleEnd { get; private set; }

        // 메타
        /// <summary>천장/보정 규칙(JSON 문자열 저장; DB jsonb 매핑 예정)</summary>
        public string? PityJson { get; private set; }
        /// <summary>확률표 버전 라벨(스냅샷 키)</summary>
        public string? TablesVersion { get; private set; }
        /// <summary>기타 설정(JSON): 비용, 1/10연, 등급 확률표 등</summary>
        public string? Config { get; private set; }

        // 엔트리(등급/가중치 표)
        private readonly List<GachaPoolEntry> _entries = new();
        public IReadOnlyList<GachaPoolEntry> Entries => _entries;

        // ───────────────────────── 생성/팩토리 ─────────────────────────
        public static GachaPool Create(
            string name,
            DateTimeOffset? scheduleStart = null,
            DateTimeOffset? scheduleEnd = null,
            string? pityJson = null,
            string? tablesVersion = null,
            string? configJson = null,
            IEnumerable<GachaPoolEntry>? entries = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("name is required", nameof(name));

            var start = scheduleStart ?? DateTimeOffset.UtcNow;
            if (scheduleEnd is { } e && e <= start)
                throw new ArgumentException("ScheduleEnd must be greater than ScheduleStart.", nameof(scheduleEnd));

            var p = new GachaPool
            {
                Name = name.Trim(),
                ScheduleStart = start,
                ScheduleEnd = scheduleEnd,
                PityJson = pityJson,
                TablesVersion = tablesVersion,
                Config = configJson
            };

            if (entries != null) p.ReplaceEntries(entries);
            return p;
        }

        // ───────────────────────── 명령(수정) ─────────────────────────
        public void Rename(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("name is required", nameof(name));
            Name = name.Trim();
        }

        public void Reschedule(DateTimeOffset start, DateTimeOffset? end)
        {
            if (end is { } e && e <= start)
                throw new ArgumentException("ScheduleEnd must be greater than ScheduleStart.", nameof(end));
            ScheduleStart = start;
            ScheduleEnd = end;
        }

        public void SetPityJson(string? json) => PityJson = json;
        public void SetTablesVersion(string? version) => TablesVersion = version;
        public void SetConfigJson(string? json) => Config = json;

        /// <summary>엔트리 한 건 업서트(동일 CharacterId 존재 시 교체)</summary>
        public void UpsertEntry(int characterId, short grade, bool rateUp, int weight)
        {
            if (characterId <= 0) throw new ArgumentOutOfRangeException(nameof(characterId));
            if (weight <= 0) throw new ArgumentOutOfRangeException(nameof(weight), "weight must be positive.");

            var existing = _entries.FirstOrDefault(e => e.CharacterId == characterId);
            if (existing is null)
                _entries.Add(GachaPoolEntry.Create(characterId, grade, rateUp, weight));
            else
                existing.Update(grade, rateUp, weight);
        }

        public void RemoveEntry(int characterId)
        {
            _entries.RemoveAll(e => e.CharacterId == characterId);
        }

        /// <summary>전체 교체(트랜잭션 단위에서 호출 권장)</summary>
        public void ReplaceEntries(IEnumerable<GachaPoolEntry> entries)
        {
            var list = entries?.ToList() ?? new();
            if (list.Any(e => e.Weight <= 0))
                throw new ArgumentException("All weights must be positive.", nameof(entries));
            if (list.Select(e => e.CharacterId).Distinct().Count() != list.Count)
                throw new ArgumentException("Duplicate CharacterId exists in entries.", nameof(entries));

            _entries.Clear();
            _entries.AddRange(list);
        }

        // 편의: 등급별 가중치 합(서버 추첨 로직 보조)
        public int SumWeightByGrade(short grade) => _entries.Where(e => e.Grade == grade).Sum(e => e.Weight);
        public int SumWeightAll() => _entries.Sum(e => e.Weight);
    }
}
