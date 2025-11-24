using Application.Common.Interface;
using Application.Repositories;
using Application.UserCurrency;
using Application.UserInventory;
using Application.Users;
using Domain.Entities.Contents;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.StageReward
{
    public class StageRewardService : IStageRewardService
    {
        private readonly IStageQueryRepository _stageQuery;
        private readonly IUserStageProgressService _progressService;
        private readonly IWalletService _wallet;
        private readonly IUserInventoryService _inventory;
        private readonly IDistributedLock _lock;
        // ItemId → 통화 코드 매핑 (예시)
        private readonly IReadOnlyDictionary<int, string> _itemToCurrencyCode =
            new Dictionary<int, string>
            {
                { 1001, "GOLD" },
                { 1002, "GEM"  },
                { 1003, "TOKEN" },
            };

        public StageRewardService(
         IStageQueryRepository stageQuery,
         IUserStageProgressService progressService,
         IWalletService wallet,
         IUserInventoryService inventory,
         IDistributedLock redisLock)
        {
            _stageQuery = stageQuery;
            _progressService = progressService;
            _wallet = wallet;
            _inventory = inventory;
            _lock = redisLock;
        }
        private async Task<StageRewardResult> GrantRewardsInternalAsync(int userId, int stageId, bool success, StageStars stars, DateTime nowUtc, CancellationToken ct)
        { 
            // 1) 스테이지 마스터 조회
            var stage = await _stageQuery.GetDetailAsync(stageId, ct)
                        ?? throw new InvalidOperationException("STAGE_NOT_FOUND");

            // 2) 기존 진행도 조회 (첫 클리어 판정용)
            var prevProgress = await _progressService.GetProgressAsync(userId, stageId, ct);
            bool wasClearedBefore = prevProgress?.Cleared == true;

            // 3) 이번 전투 결과 기록 (UserStageProgress에 반영)
            var updatedProgress = await _progressService.MarkStageFinishedAsync(
                userId: userId,
                stageId: stageId,
                success: success,
                stars: stars,
                nowUtc: nowUtc,
                ct: ct);

            // 4) 실패면 보상 없음
            if (!success)
            {
                return new StageRewardResult(
                    StageId: stageId,
                    Success: false,
                    IsFirstClear: false,
                    Rewards: Array.Empty<GainedRewardDto>(),
                    Gold: 0,
                    Gem: 0,
                    Token: 0
                );
            }

            // 5) 첫 클리어 여부
            //   - 이전에 클리어한 적 없었고
            //   - 이번에 Cleared가 true로 바뀐 경우
            bool isFirstClear = !wasClearedBefore && updatedProgress.Cleared;

            var rewards = new List<GainedRewardDto>();
            var rng = new Random();
            // 6) 첫 클리어 고정 보상
            if (isFirstClear)
            {
                foreach (var r in stage.FirstRewards)
                {
                    if (r.Qty <= 0) continue;

                    rewards.Add(new GainedRewardDto(
                        ItemId: r.ItemId,
                        Qty: r.Qty,
                        FirstClearReward: true));
                }
            }

            // 4-2) 드랍 테이블
            foreach (var d in stage.Drops)
            {
                if (d.FirstClearOnly && !isFirstClear)
                    continue;

                if (!Roll(rng, d.Rate))
                    continue;

                var qty = RandomQty(rng, d.MinQty, d.MaxQty);
                if (qty <= 0)
                    continue;

                rewards.Add(new GainedRewardDto(
                    ItemId: d.ItemId,
                    Qty: qty,
                    FirstClearReward: d.FirstClearOnly));
            }

            // 5) 통화형 아이템을 Wallet에 지급 + 합계 계산
            var (gold, gem, token) = await GrantCurrenciesAsync(userId, rewards, ct);

            await GrantInventoryItemsAsync(userId, rewards, ct);
            return new StageRewardResult(
                StageId: stageId,
                Success: true,
                IsFirstClear: isFirstClear,
                Rewards: rewards,
                Gold: gold,
                Gem: gem,
                Token: token
            );
        }
        public async Task<StageRewardResult> GrantRewardsAsync(int userId, int stageId, bool success, StageStars stars, DateTime nowUtc, CancellationToken ct = default)
        {
            string lockKey = $"lock:stage-reward:{userId}:{stageId}";

            if (!await _lock.AcquireAsync(lockKey, TimeSpan.FromSeconds(3)))
                throw new InvalidOperationException("STAGE_REWARD_BUSY");

            try
            {
                return await GrantRewardsInternalAsync(userId, stageId, success, stars, nowUtc, ct);
            }
            finally
            {
                await _lock.ReleaseAsync(lockKey);
            } 
        }

        // ----------------- 내부 유틸 -----------------
        private async Task GrantInventoryItemsAsync(int userId, IReadOnlyList<GainedRewardDto> rewards, CancellationToken ct)
        {
            foreach (var r in rewards)
            {
                if (_itemToCurrencyCode.ContainsKey(r.ItemId))
                    continue;   // 통화 아이템은 건너뜀

                var req = new GrantItemRequest(
                    UserId: userId,
                    ItemId: r.ItemId,
                    Amount: (int)r.Qty
                );

                await _inventory.GrantAsync(req, ct);
            }
        }
        private static bool Roll(Random rng, decimal rate)
        {
            // rate: 0.0 ~ 1.0 가정
            if (rate <= 0) return false;
            if (rate >= 1) return true;

            var r = (decimal)rng.NextDouble();
            return r <= rate;
        }

        private static long RandomQty(Random rng, short min, short max)
        {
            if (max < min) max = min;
            if (min <= 0 && max <= 0) return 0;

            return rng.Next(min, max + 1);
        }

        private async Task<(long Gold, long Gem, long Token)> GrantCurrenciesAsync(
            int userId,
            IReadOnlyList<GainedRewardDto> rewards,
            CancellationToken ct)
        {
            var byCode = new Dictionary<string, long>();

            foreach (var r in rewards)
            {
                if (!_itemToCurrencyCode.TryGetValue(r.ItemId, out var code))
                    continue;

                if (!byCode.TryGetValue(code, out var cur))
                    cur = 0;

                byCode[code] = cur + r.Qty;
            }

            long gold = 0, gem = 0, token = 0;

            foreach (var kv in byCode)
            {
                var code = kv.Key;
                var amt = kv.Value;

                if (amt <= 0) continue;

                await _wallet.GrantAsync(userId, code, amt, ct);

                switch (code)
                {
                    case "GOLD": gold = amt; break;
                    case "GEM": gem = amt; break;
                    case "TOKEN": token = amt; break;
                }
            }

            return (gold, gem, token);
        }
    }
}
