using Application.GachaBanner;
using Application.GachaPool;
using Contracts.Protos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("api/pb/gacha")]
    [Authorize]
    [Produces("application/x-protobuf")]
    public sealed class GachaProtoController : ControllerBase
    {
        private readonly IGachaBannerService _banners;
        private readonly IGachaPoolService _pools;

        public GachaProtoController(IGachaBannerService banners, IGachaPoolService pools)
        {
            _banners = banners;
            _pools = pools;
        }

        // ─────────────────────────────────────────────────────────────
        // 1) 활성 배너 목록만 (가볍게)
        // GET /api/pb/gacha/active-banners
        // ─────────────────────────────────────────────────────────────
        [HttpGet("active-banners")]
        public async Task<ActionResult<GachaBannerListPb>> GetActiveBanners(CancellationToken ct)
        {
            // Service에 ListLiveAsync(take) 유틸이 이미 있을 것으로 보임
            var items = await _banners.ListLiveAsync(50, ct);

            var res = new GachaBannerListPb();
            foreach (var b in items)
            {
                res.Banners.Add(MapBanner(b));
            }
            return Ok(res);
        }

        // ─────────────────────────────────────────────────────────────
        // 2) 특정 풀 상세(엔트리 포함)
        // GET /api/pb/gacha/pools/{poolId}
        // ─────────────────────────────────────────────────────────────
        [HttpGet("pools/{poolId:int}")]
        public async Task<ActionResult<GachaPoolDetailPb>> GetPoolDetail([FromRoute] int poolId, CancellationToken ct)
        {
            var dto = await _pools.GetDetailAsync(poolId, ct);
            if (dto is null) return NotFound();

            return Ok(MapPool(dto));
        }

        // ─────────────────────────────────────────────────────────────
        // 3) 카탈로그(한 방에): 활성 배너 + 참조 풀(중복 제거) 세트
        // GET /api/pb/gacha/catalog
        //  - 클라가 이 응답 하나만으로 로비 배너/풀 UI 구성 가능
        // ─────────────────────────────────────────────────────────────
        [HttpGet("catalog")]
        public async Task<ActionResult<GachaCatalogPb>> GetCatalog(CancellationToken ct)
        {
            var active = await _banners.ListLiveAsync(50, ct);

            // 배너 → 고유 풀 ID 집합
            var poolIds = active.Select(b => b.GachaPoolId).Distinct().ToList();

            // 풀 상세 병렬 로드
            var poolTasks = poolIds.Select(id => _pools.GetDetailAsync(id, ct)).ToArray();
            var poolDtos = await Task.WhenAll(poolTasks);

            var res = new GachaCatalogPb();
            foreach (var b in active)
                res.Banners.Add(MapBanner(b));

            foreach (var p in poolDtos.Where(p => p is not null)!)
                res.Pools.Add(MapPool(p!));

            // 버전필드(선택) : 클라 캐싱전략에 사용
            res.Version = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return Ok(res);
        }

        // ─────────────────────────────────────────────────────────────
        // 매핑 헬퍼들
        // ─────────────────────────────────────────────────────────────
        private static GachaBannerPb MapBanner(GachaBannerDto b) => new()
        {
            Id = b.Id,
            Key = b.Key ?? string.Empty,
            Title = b.Title ?? string.Empty,
            Subtitle = b.Subtitle ?? string.Empty,
            PortraitId = b.PortraitId ?? 0,
            GachaPoolId = b.GachaPoolId,
            StartsAtUtc = b.StartsAt.ToUnixTimeSeconds(),
            EndsAtUtc = b.EndsAt?.ToUnixTimeSeconds() ?? 0,
            Priority = b.Priority,
            Status = (int)b.Status, // enum은 int로 내려서 클라가 매핑
            IsActive = b.IsActive
        };
        private static GachaPoolDetailPb MapPool(Application.GachaPool.GachaPoolDetailDto p)
        {
            // 리플렉션 유틸
            object? Get(string name) => p.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)?.GetValue(p);

            // 여러 후보 이름을 순서대로 시도하는 헬퍼
            T? Pick<T>(params string[] names)
            {
                foreach (var n in names)
                {
                    var val = Get(n);
                    if (val is null) continue;
                    if (val is T t) return t;

                    // DateTime/DateTimeOffset 같은 경우 형변환 보조
                    if (typeof(T) == typeof(DateTimeOffset?) && val is DateTime dt)
                        return (T?)(object?)new DateTimeOffset(DateTime.SpecifyKind(dt, DateTimeKind.Utc));
                    if (typeof(T) == typeof(DateTimeOffset?) && val is DateTimeOffset dto)
                        return (T?)(object?)dto;
                    if (typeof(T) == typeof(string) && val is not null)
                        return (T?)(object?)val.ToString();
                }
                return default;
            }

            // 필드 가져오기(여러 후보명 대응)
            var poolId = Pick<int>("PoolId", "Id")!;
            var name = Pick<string>("Name") ?? string.Empty;
            var tablesVersion = Pick<string>("TablesVersion", "TableVersion", "TablesVer") ?? string.Empty;
            var pityJson = Pick<string>("PityJson", "Pity") ?? string.Empty;
            var configJson = Pick<string>("ConfigJson", "Config") ?? string.Empty;

            var startOpt = Pick<DateTimeOffset?>("ScheduleStart", "StartsAt", "StartAt");
            var endOpt = Pick<DateTimeOffset?>("ScheduleEnd", "EndsAt", "EndAt");

            long startUnix = startOpt?.ToUnixTimeSeconds() ?? 0;
            long endUnix = endOpt?.ToUnixTimeSeconds() ?? 0;

            var pb = new GachaPoolDetailPb
            {
                PoolId = poolId,
                Name = name,
                TablesVersion = tablesVersion,
                PityJson = pityJson,
                ConfigJson = configJson,
                ScheduleStartUtc = startUnix,
                ScheduleEndUtc = endUnix
            };

            // Entries 컬렉션(이름은 보통 "Entries")
            var entriesObj = Get("Entries") as System.Collections.IEnumerable;
            if (entriesObj is not null)
            {
                foreach (var e in entriesObj)
                {
                    object? GetE(string prop) => e.GetType().GetProperty(prop, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)?.GetValue(e);

                    int characterId = (int?)(GetE("CharacterId") ?? 0) ?? 0;
                    int grade = (int?)(GetE("Grade") ?? 0) ?? 0;
                    bool rateUp = (bool?)(GetE("RateUp") ?? false) ?? false;
                    int weight = (int?)(GetE("Weight") ?? 0) ?? 0;

                    pb.Entries.Add(new GachaEntryPb
                    {
                        CharacterId = characterId,
                        Grade = grade,
                        RateUp = rateUp,
                        Weight = weight
                    });
                }
            }

            return pb;
        }
    }
}
