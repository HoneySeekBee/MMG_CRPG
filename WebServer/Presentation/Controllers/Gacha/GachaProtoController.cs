using Application.Gacha.GachaBanner;
using Application.Gacha.GachaPool;
using Contracts.Protos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace WebServer.Controllers.Gacha
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

        // 1) 활성 배너 목록만 (가볍게)
        // GET /api/pb/gacha/active-banners
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

        // 2) 특정 풀 상세(엔트리 포함)
        // GET /api/pb/gacha/pools/{poolId}
        [HttpGet("pools/{poolId:int}")]
        public async Task<ActionResult<GachaPoolDetailPb>> GetPoolDetail([FromRoute] int poolId, CancellationToken ct)
        {
            var dto = await _pools.GetDetailAsync(poolId, ct);
            if (dto is null) return NotFound();

            return Ok(MapPool(dto));
        }

        // 3) 카탈로그(한 방에): 활성 배너 + 참조 풀(중복 제거) 세트
        // GET /api/pb/gacha/catalog
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

        // 매핑 헬퍼들
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
            Status = (int)b.Status,  
            IsActive = b.IsActive,
            CostCurrencyId = b.CostCurrencyId,
            Cost = b.Cost,
            TicketItemId = b.TicketItemId
        };
        private static GachaPoolDetailPb MapPool(GachaPoolDetailDto p)
        {
            var pb = new GachaPoolDetailPb
            {
                PoolId = p.Pool.PoolId,
                Name = p.Pool.Name ?? string.Empty,
                TablesVersion = p.Pool.TablesVersion ?? string.Empty,
                PityJson = p.PityJson ?? string.Empty,
                ConfigJson = p.ConfigJson ?? string.Empty,
                ScheduleStartUtc = p.Pool.ScheduleStart.ToUnixTimeSeconds(),
                ScheduleEndUtc = p.Pool.ScheduleEnd?.ToUnixTimeSeconds() ?? 0
            };

            foreach (var e in p.Entries)
            {
                pb.Entries.Add(new GachaEntryPb
                {
                    CharacterId = e.CharacterId,
                    Grade = e.Grade,     
                    RateUp = e.RateUp,
                    Weight = e.Weight
                });
            }

            return pb;
        }
    }
}
