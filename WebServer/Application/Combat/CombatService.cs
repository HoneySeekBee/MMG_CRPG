using Application.Contents.Stages;
using Application.Repositories;
using Application.Skills;
using Application.StageReward;
using Application.UserCharacter;
using Application.UserCurrency;
using Application.UserParties;
using Application.Users;
using Domain.Entities.Contents;
using Domain.Enum;
using Domain.Services;
using Domain.Combat.Engine;
using Domain.Entities.Combats;
using CombatEntitiy = Domain.Entities.Combats.Combat;

namespace Application.Combat
{

    public sealed class CombatService : ICombatService
    {
        private readonly IMasterDataProvider _master;
        private readonly ICombatRepository _repo;
        private readonly ICombatEngine _engine;
        private readonly IUserPartyReader _partyReader;
        private readonly IUserCharacterReader _userCharacterReader;

        // 전투 종료시 사용되는 서비스
        private readonly IUserStageProgressService _stageProgress;
        private readonly IStageRewardService _stageReward;
        private readonly IWalletService _wallet;
        private readonly IStagesService _stages;
        private readonly IClock _clock;
        private readonly ISkillCache _skillCache;
        private readonly CombatServerClient _combatServerClient;

        private const int MaxPageSize = 500;

        public CombatService(IMasterDataProvider master, ICombatRepository repo, ICombatEngine engine, IUserPartyReader partyReader,
       IUserCharacterReader userCharacterReader, IUserStageProgressService stageProgress,
       IStageRewardService stageReward, IWalletService wallet, IStagesService stages, IClock clock, ISkillCache skillCache,
       CombatServerClient combatServerClient)
        {
            _master = master;
            _repo = repo;
            _engine = engine;
            _partyReader = partyReader;
            _userCharacterReader = userCharacterReader;

            _stageProgress = stageProgress;
            _stageReward = stageReward;
            _wallet = wallet;
            _stages = stages;
            _clock = clock;
            _skillCache = skillCache;
            _combatServerClient = combatServerClient;
        }
        public async Task<StartCombatResponse> StartAsync(StartCombatRequest req, CancellationToken ct)
        {
            if (req.StageId <= 0)
                throw new ArgumentException("StageId must be positive.", nameof(req.StageId));

            // (1) 유저 파티 로드하기  
            var party = await _partyReader.GetByUserBattleAsync(req.UserId, req.BattleId, ct)
                        ?? throw new InvalidOperationException($"Party {req.UserId} not found.");

            var filledSlots = party.Slots
                .Where(s => s.UserCharacterId.HasValue)
                .OrderBy(s => s.SlotId)
                .ToList();

            if (filledSlots.Count == 0)
                throw new InvalidOperationException($"Party {req.BattleId} has no members.");

            //  (2) 파티에 포함된 UserCharacterId 목록 
            var partyCharacterIds = filledSlots
                .Select(s => (long)s.UserCharacterId!.Value)
                .Distinct()
                .ToArray();

            // (3) 유저 캐릭터 + 레벨별 스탯 로드 
            var userCharStats = await _userCharacterReader
                .GetManyByCharacterIdAsync(partyCharacterIds, req.UserId, ct);

            var statsByCharacterId = userCharStats
                .ToDictionary(x => (long)x.CharacterId);

            var masterCharIds = partyCharacterIds;

            //  (5) 마스터 데이터 패키지 (스테이지 + 캐릭터 마스터) 
            var pack = await _master.BuildPackAsync(req.StageId, req.UserId, masterCharIds, ct);

            // (6) CombatInputSnapshot 생성 (PartyMember: 마스터 캐릭터ID + 유저레벨)
            var partyMembers = filledSlots.Select(s =>
            {
                long charId = s.UserCharacterId!.Value;

                if (!statsByCharacterId.TryGetValue(charId, out var uc))
                    throw new InvalidOperationException($"CharacterId {charId} not found in stats.");

                return new PartyMember(
                    CharacterId: uc.CharacterId,
                    Level: uc.Level
                );
            }).ToArray();

            var input = new CombatInputSnapshot(
                req.StageId,
                partyMembers,
                Array.Empty<SkillInput>());

            // (7) Combat Aggregate 생성/저장
            var seed = BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 0);
            if (seed == 0) seed = 1;

            var combat = CombatEntitiy.Create(
                CombatMode.Pve,
                req.StageId,
                seed,
                input,
                balanceVersion: "1",
                clientVersion: null
            );

            var combatId = await _repo.SaveAsync(
                combat,
                events: Enumerable.Empty<Domain.Events.CombatLogEvent>(),
                ct
            );

            // (8) Build payload and send to CombatServer
            var players = filledSlots.Select((slot, idx) =>
            {
                long charId = slot.UserCharacterId!.Value;
                var uc = statsByCharacterId[charId];
                return (SlotId: slot.SlotId, CharacterId: (long)uc.CharacterId, Hp: uc.Hp);
            });

            var payload = CombatServerPayloadMapper.Build(combatId, req.StageId, req.UserId, seed, players, pack);
            var csSnapshot = await _combatServerClient.InitCombatAsync(payload, ct);

            // (9) Map CombatServer response to domain DTO
            var actors = csSnapshot.Actors.Select(a => new ActorInitDto(
                ActorId: a.ActorId,
                Team: a.Team,
                X: a.X,
                Z: a.Z,
                Hp: a.Hp,
                WaveIndex: a.WaveIndex,
                MasterId: a.MasterId
            )).ToList();

            var snapshot = new CombatInitialSnapshotDto(actors);
            return new StartCombatResponse(combatId, snapshot);
        }
        public async Task<SimulateCombatResponse> SimulateAsync(SimulateCombatRequest req, CancellationToken ct)
        {
            // 1) 검증
            if (req.Party is null || req.Party.Count == 0)
                throw new ArgumentException("Party is required.");

            // 2) 시드 결정(0 방지)
            var seed = req.Seed ?? BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 0);
            if (seed == 0) seed = 1;

            // 3) 도메인 입력 스냅샷
            var party = req.Party
                .Select(p => new PartyMember(p.CharacterId, p.Level))
                .ToArray();

            var skills = (req.SkillInputs ?? Enumerable.Empty<SkillInputDto>())
                .Select(s => new SkillInput(s.TMs, s.CasterRef, s.SkillId, s.Targets.ToArray()))
                .ToArray();

            var input = new CombatInputSnapshot(req.StageId, party, skills);

            // 4) 마스터 데이터 패키지
            var partyIds = party.Select(x => x.CharacterId).ToArray();
            var masterPack = await _master.BuildEnginePackAsync(req.StageId, partyIds, ct);

            // 5) Aggregate 생성
            var combat = CombatEntitiy.Create(
                Domain.Enum.CombatMode.Pve, req.StageId, seed, input,
                balanceVersion: "1", // TODO: 운영툴/설정에서 주입
                clientVersion: req.ClientVersion);

            // 6) 전투 시뮬레이션 
            var result = _engine.Simulate(input, seed, masterPack);

            // 7) 결과 반영
            switch (result.Result)
            {
                case Domain.Enum.CombatResult.Win: combat.CompleteWin(result.ClearMs); break;
                case Domain.Enum.CombatResult.Lose: combat.CompleteLose(result.ClearMs); break;
                default: combat.CompleteError(); break;
            }

            // 8) 저장(트랜잭션 내 combat + logs)
            var combatId = await _repo.SaveAsync(combat, result.Events, ct);

            // 9) 응답 매핑 (이벤트 일부만)  메서드 그룹 대신 람다 사용
            var eventsShort = result.Events.Take(200).Select(e => Map(e)).ToList();

            return new SimulateCombatResponse(
                combatId,
                result.Result.ToString().ToLowerInvariant(),
                combat.ClearMs,
                combat.BalanceVersion,
                combat.ClientVersion,
                eventsShort
            );
        }
        public async Task<FinishCombatResponse> FinishAsync(FinishCombatRequest req, CancellationToken ct)
        {
            // (1) Get combat result from CombatServer (also removes from CombatServer memory)
            var combatResult = await _combatServerClient.GetResultAsync(req.CombatId, ct);

            // (2) Validate ownership
            if (combatResult.UserId != req.UserId)
                throw new InvalidOperationException("COMBAT_USER_MISMATCH");

            if (combatResult.Result is null)
                throw new InvalidOperationException("COMBAT_RESULT_MISSING");

            // (3) Calculate stars from survivor data
            bool success = combatResult.Result == CombatResult.Win;
            StageStars stars = CalculateStars(success, combatResult.DeadPlayerCount);

            // (4) Grant rewards
            var rewardResult = await _stageReward.GrantRewardsAsync(
                userId: req.UserId,
                stageId: combatResult.StageId,
                success: success,
                stars: stars,
                nowUtc: _clock.UtcNow.UtcDateTime,
                ct: ct);

            // (5) Map to response DTO
            var items = rewardResult.Rewards
                .Select(r => new GainedItemDto(
                    ItemId: r.ItemId,
                    Qty: (int)r.Qty,
                    IsFirstClearReward: r.FirstClearReward))
                .ToList();

            return new FinishCombatResponse(
                StageId: rewardResult.StageId,
                Stars: stars,
                FirstClear: rewardResult.IsFirstClear,
                Items: items,
                Gold: rewardResult.Gold,
                Gem: rewardResult.Gem,
                Token: rewardResult.Token,
                Result: combatResult.Result.Value);
        }

        private static StageStars CalculateStars(bool success, int deadPlayerCount)
        {
            if (!success) return StageStars.Zero;
            if (deadPlayerCount <= 0) return StageStars.Three;
            if (deadPlayerCount < 3) return StageStars.Two;
            return StageStars.One;
        }
        // Map 메서드는 딱 하나만 남긴다
        private static CombatLogEventDto Map(Domain.Events.CombatLogEvent e)
            => new(e.TMs, e.Type, e.Actor, e.Target, e.Damage, e.Crit, e.Extra);

        public async Task<CombatLogPageDto> GetLogAsync(long combatId, string? cursor, int size, CancellationToken ct)
        {
            if (size <= 0) size = 100;
            size = Math.Min(size, MaxPageSize);
            return await _repo.GetLogAsync(combatId, cursor, size, ct);
        }

        public Task<CombatLogSummaryDto> GetSummaryAsync(long combatId, CancellationToken ct)
            => _repo.GetSummaryAsync(combatId, ct);

    }
}
