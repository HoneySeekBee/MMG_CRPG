using Application.Combat.Engine;
using Application.Combat.Runtime;
using Application.Contents.Stages;
using Application.Repositories;
using Application.StageReward;
using Application.UserCharacter;
using Application.UserCurrency;
using Application.UserParties;
using Application.Users;
using Domain.Entities.Contents;
using Domain.Enum;
using Domain.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Application.Combat
{

    public sealed class CombatService : ICombatService
    {
        private readonly IMasterDataProvider _master;
        private readonly ICombatRepository _repo;
        private readonly ICombatEngine _engine;
        private readonly IUserPartyReader _partyReader;
        private readonly IUserCharacterReader _userCharacterReader;
        private readonly ICombatTickEngine _tickEngine;

        // 전투 종료시 사용되는 서비스 
        private readonly IUserStageProgressService _stageProgress;
        private readonly IStageRewardService _stageReward;
        private readonly IWalletService _wallet;
        private readonly IStagesService _stages;
        private readonly IClock _clock;


        private const int MaxPageSize = 500;
        private static readonly ConcurrentDictionary<long, CombatRuntimeState> _runtimeStates = new();

        public CombatService(
       IMasterDataProvider master,
       ICombatRepository repo,
       ICombatEngine engine,
       IUserPartyReader partyReader,
       IUserCharacterReader userCharacterReader,
       ICombatTickEngine tickEngine,
       IUserStageProgressService stageProgress,
       IStageRewardService stageReward,
       IWalletService wallet,
       IStagesService stages,
       IClock clock)
        {
            _master = master;
            _repo = repo;
            _engine = engine;
            _partyReader = partyReader;
            _userCharacterReader = userCharacterReader;
            _tickEngine = tickEngine;

            _stageProgress = stageProgress;
            _stageReward = stageReward;
            _wallet = wallet;
            _stages = stages;
            _clock = clock;
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

                return new Domain.Entities.PartyMember(
                    CharacterId: uc.CharacterId,
                    Level: uc.Level
                );
            }).ToArray();

            var input = new Domain.Entities.CombatInputSnapshot(
                req.StageId,
                partyMembers,
                Array.Empty<Domain.Entities.SkillInput>());

            // (7) Combat Aggregate 생성/저장
            var seed = BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 0);
            if (seed == 0) seed = 1;

            var combat = Domain.Entities.Combat.Create(
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

            // 7-1) RuntimeState 먼저 생성
            _runtimeStates[combatId] = new CombatRuntimeState
            {
                CombatId = combatId,
                StageId = req.StageId,
                UserId = req.UserId,
                Seed = seed,
                StartedAt = DateTimeOffset.UtcNow,
                CurrentWaveIndex = 1,
                TotalWaves = pack.Stage.Waves.Count
            };

            var runtimeState = _runtimeStates[combatId];
            runtimeState.Snapshot = new CombatRuntimeSnapshot();
            runtimeState.MasterPack = pack;

            // (8) ActorInitDto 구성
            var actors = new List<ActorInitDto>();

            // Player actors
            foreach (var slot in filledSlots)
            {
                long charId = slot.UserCharacterId!.Value;
                var uc = statsByCharacterId[charId];

                var (x, z) = PositionUtils.GetPlayerPositionBySlot(slot.SlotId);
                var actorId = 1 + slot.SlotId;

                actors.Add(new ActorInitDto(
                    ActorId: actorId,
                    Team: 0,
                    X: x,
                    Z: z,
                    Hp: uc.Hp,
                    WaveIndex: 1,
                    MasterId: uc.CharacterId
                ));
            }

            // Enemy actors
            foreach (var wave in pack.Stage.Waves)
            {
                foreach (var spawn in wave.Enemies)
                {
                    var (x, z) = PositionUtils.GetEnemyPositionBySlot(spawn.Slot);
                    var actorId = 1000 * wave.Index + spawn.Slot;

                    long cid = spawn.MonsterId;
                    var cdef = pack.Actors[cid];

                    actors.Add(new ActorInitDto(
                        ActorId: actorId,
                        Team: 1,
                        X: x,
                        Z: z,
                        Hp: cdef.MaxHp,
                        WaveIndex: wave.Index,
                        MasterId: cid
                    ));
                }
            }

            // (9) actors 전체를 Snapshot에 로드
            foreach (var a in actors)
            {
                var def = pack.Actors[a.MasterId];
                runtimeState.Snapshot.Actors[a.ActorId] = new ActorState
                {
                    ActorId = a.ActorId,
                    Team = a.Team,
                    X = a.X,
                    Z = a.Z,

                    SpawnX = a.X,
                    SpawnZ = a.Z,

                    Hp = a.Hp,
                    Atk = def.Atk,
                    Def = def.Def,
                    Spd = def.Spd,
                    Range = def.Range,
                    AttackIntervalMs = def.AttackIntervalMs,
                    CritRate = def.CritRate,
                    CritDamage = def.CritDamage,
                    AttackCooldownMs = 0,
                    SkillCooldownMs = 0,
                    TargetActorId = null,
                    Waveindex = a.WaveIndex
                };
            }

            // (9-1) ActiveActors 초기화 - 전투 시작 시점에 필드에 있는 애들 등록
            // CombatRuntimeState 안에 ActiveActors가 Dictionary<long, ActorState> 라고 가정
            runtimeState.ActiveActors.Clear();

            // 플레이어는 항상 필드에 존재하므로 전부 ActiveActors 에 넣어줌
            foreach (var a in runtimeState.Snapshot.Actors.Values)
            {
                if (a.Waveindex == runtimeState.CurrentWaveIndex && a.Hp > 0)
                {
                    runtimeState.ActiveActors[a.ActorId] = a;
                }
            }
            // (10) 클라이언트 초기 스냅샷 반환
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
                .Select(p => new Domain.Entities.PartyMember(p.CharacterId, p.Level))
                .ToArray();

            var skills = (req.SkillInputs ?? Enumerable.Empty<SkillInputDto>())
                .Select(s => new Domain.Entities.SkillInput(s.TMs, s.CasterRef, s.SkillId, s.Targets.ToArray()))
                .ToArray();

            var input = new Domain.Entities.CombatInputSnapshot(req.StageId, party, skills);

            // 4) 마스터 데이터 패키지
            var partyIds = party.Select(x => x.CharacterId).ToArray();
            var masterPack = await _master.BuildEnginePackAsync(req.StageId, partyIds, ct);

            // 5) Aggregate 생성
            var combat = Domain.Entities.Combat.Create(
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
            if (!_runtimeStates.TryGetValue(req.CombatId, out var state))
                throw new KeyNotFoundException($"Combat {req.CombatId} not found");

            // (1) 다른 유저가 Finish 못하게
            if (state.UserId != req.UserId)
                throw new InvalidOperationException("COMBAT_USER_MISMATCH");

            // (2) 아직 서버 상태에서 전투가 안 끝났으면 막기
            if (!state.BattleEnded)
                throw new InvalidOperationException("COMBAT_NOT_FINISHED");

            // (3) 승패/별 계산 
            bool success = true; 
            StageStars stars = CalculateStars(state, success);

            // (4) 보상 + 진행도 + 지갑 처리 전부 StageRewardService에 위임
            var rewardResult = await _stageReward.GrantRewardsAsync(
      userId: req.UserId,
      stageId: state.StageId,
      success: success,
      stars: stars,
      nowUtc: _clock.UtcNow.UtcDateTime,
      ct: ct);

            // (5) runtime state 정리 (메모리 누수 방지)
            _runtimeStates.TryRemove(req.CombatId, out _);

            // (6) 클라로 보낼 DTO로 매핑 
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
        Token: rewardResult.Token
    );
        }
        private StageStars CalculateStars(CombatRuntimeState state, bool success)
        {
            if (!success)
                return StageStars.Zero;

            var players = state.Snapshot.Actors.Values
                .Where(a => a.Team == 0)
                .ToList();

            int deadCount = players.Count(a => a.Dead || a.Hp <= 0);

            if (deadCount <= 0)
                return StageStars.Three;
            if (deadCount < 3)
                return StageStars.Two;
            return StageStars.One;
        }
        public async Task EnqueueCommandAsync(long combatId, CombatCommandDto cmd, CancellationToken ct)
        {
            if (!_runtimeStates.TryGetValue(combatId, out var state))
            {
                throw new KeyNotFoundException($"Combat runtime state not found for id {combatId}");
            }

            lock (state.SyncRoot)
            {
                state.PendingCommands.Enqueue(cmd);
            }
            var tMs = (int)(DateTimeOffset.UtcNow - state.StartedAt).TotalMilliseconds;

            var ev = new Domain.Events.CombatLogEvent(
          TMs: tMs,
          Type: "skill_used",
          Actor: cmd.ActorId.ToString(),              // 일단 string ActorId
          Target: cmd.TargetActorId?.ToString(),      // 없으면 null
          Damage: null,
          Crit: null,
          Extra: new Dictionary<string, object?>
          {
              ["skillId"] = cmd.SkillId
          }
      );

            await _repo.AppendLogsAsync(combatId, new[] { ev }, ct);
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

        public async Task<CombatTickResponse> TickAsync(long combatId, int tick, CancellationToken ct)
        {
            if (!_runtimeStates.TryGetValue(combatId, out var state))
                throw new KeyNotFoundException($"Combat {combatId} not found");

            List<CombatLogEventDto> evs;
            CombatSnapshotDto snapshot;

            lock (state.SyncRoot)
            {
                // 1) 틱 처리 → 이벤트 수집
                evs = _tickEngine.Process(state);

                // 2) 현재 상태 기준 스냅샷 
                snapshot = _tickEngine.BuildSnapshot(state);
            }

            // 3) 이벤트 로그 영속화
            if (evs.Count > 0)
            {
                var domainEvents = evs.Select(e =>
                    new Domain.Events.CombatLogEvent(
                        TMs: e.TMs,
                        Type: e.Type,
                        Actor: e.Actor,
                        Target: e.Target,
                        Damage: e.Damage,
                        Crit: e.Crit,
                        Extra: e.Extra
                    ));

                await _repo.AppendLogsAsync(combatId, domainEvents, ct);
            }

            // 4) 스냅샷 + 이벤트를 함께 반환
            return new CombatTickResponse(combatId, tick, snapshot, evs);
        }
    }
}
