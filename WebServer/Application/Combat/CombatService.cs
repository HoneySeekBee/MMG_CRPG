using Application.Repositories;
using Application.UserParties;
using Domain.Enum;
using Domain.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Application.Combat
{
    internal sealed class CombatRuntimeState
    {
        public long CombatId { get; init; }
        public int StageId { get; init; }
        public long Seed { get; init; }

        public DateTimeOffset StartedAt { get; init; } = DateTimeOffset.UtcNow;

        public int CurrentWaveIndex { get; set; } = 1;
        public int TotalWaves { get; set; }

        public CombatBattlePhase Phase { get; set; } = CombatBattlePhase.Running;
        public Queue<CombatCommandDto> PendingCommands { get; } = new();
        public object SyncRoot { get; } = new();
    }
    internal enum CombatBattlePhase
    {
        Running,            // 전투 진행 중
        WaitingNextWave,    // 웨이브 끝, 클라 신호 기다리는 중
        Completed           // 스테이지 전체 종료
    }
    public sealed class CombatService : ICombatService
    {
        private readonly IMasterDataProvider _md;
        private readonly ICombatRepository _repo;
        private readonly ICombatEngine _engine;
        private readonly IUserPartyReader _partyReader;

        private const int MaxPageSize = 500;
        private static readonly ConcurrentDictionary<long, CombatRuntimeState> _runtimeStates = new();

        public CombatService(
            IMasterDataProvider md,
            ICombatRepository repo,
            ICombatEngine engine,
            IUserPartyReader partyReader)
        {
            _md = md;
            _repo = repo;
            _engine = engine;
            _partyReader = partyReader;
        }
        public async Task<StartCombatResponse> StartAsync(StartCombatRequest req, CancellationToken ct)
        {
            // 1) 기본 검증
            if (req.StageId <= 0)
                throw new ArgumentException("StageId must be positive.", nameof(req.StageId));
            var party = await _partyReader.GetAsync(req.FormationId, ct);
            if (party is null)
                throw new InvalidOperationException($"Party {req.FormationId} not found.");

            var filledSlots = party.Slots.Where(s => s.UserCharacterId.HasValue).OrderBy(s => s.SlotId).ToList();

            if (filledSlots.Count == 0)
                throw new InvalidOperationException($"Party {req.FormationId} has no members.");

            var partyCharacterIds = filledSlots.Select(s => (long)s.UserCharacterId!.Value).ToArray();
            // 2) Seed 생성
            var seed = BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 0);
            if (seed == 0) seed = 1;

            // 3) 마스터 데이터 패키지 (스테이지 + 캐릭터들)
            var pack = await _md.BuildPackAsync(req.StageId, partyCharacterIds, ct);

            // 4) CombatInputSnapshot 생성 (PartyMember/SkillInput은 일단 최소)
            var partyMembers = partyCharacterIds
                .Select(cid => new Domain.Entities.PartyMember(cid, Level: 1)) // TODO: 실제 레벨 사용
                .ToArray();

            var input = new Domain.Entities.CombatInputSnapshot(
       req.StageId,
       partyMembers,
       Array.Empty<Domain.Entities.SkillInput>()
   );

            // 5) 도메인 Combat Aggregate 생성
            var combat = Domain.Entities.Combat.Create(
                CombatMode.Pve,
                req.StageId,
                seed,
                input,
                balanceVersion: "1",
                clientVersion: null
            );

            // 6) DB에 CombatRecord + (초기에는 로그 없음) 저장
            var combatId = await _repo.SaveAsync(combat,
                events: Enumerable.Empty<Domain.Events.CombatLogEvent>(),
                ct);


            // 7) 인메모리 런타임 상태 등록
            var runtime = new CombatRuntimeState
            {
                CombatId = combatId,
                StageId = req.StageId,
                Seed = seed,
                StartedAt = DateTimeOffset.UtcNow
            };

            _runtimeStates[combatId] = runtime;

            // 8) 초기 스냅샷 구성 (ActorInitDto 리스트) 
            var actors = new List<ActorInitDto>();

            // 8-1) 플레이어 유닛들 (왼쪽 라인)
            for (int i = 0; i < filledSlots.Count; i++)
            {
                var slot = filledSlots[i];
                var cid = (long)slot.UserCharacterId!;

                if (!pack.Characters.TryGetValue(cid, out var cdef))
                    throw new KeyNotFoundException($"CharacterDef {cid} not found in pack.");

                var (x, z) = GetPlayerPositionBySlot(slot.SlotId);
                var actorId = 1 + i; 

                actors.Add(new ActorInitDto(
                    ActorId: actorId,
                    Team: 0, // Player
                    X: x,
                    Z: z,
                    Hp: cdef.BaseHp,
                    MaxHp: cdef.BaseHp,
                    ModelCode: $"Hero_{cid}" // TODO: 실제 프리팹 코드 매핑
                ));
            }
            // 8-2) 적 유닛들 (오른쪽 라인)
            const float enemyStartX = 3.0f;
            const float spacingZ = 1.5f;

            var enemies = pack.Stage.Enemies;

            for (int i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                var cid = enemy.CharacterId;

                if (!pack.Characters.TryGetValue(cid, out var cdef))
                    throw new KeyNotFoundException($"Enemy CharacterDef {cid} not found in pack.");

                var actorId = 100 + i;

                actors.Add(new ActorInitDto(
                    ActorId: actorId,
                    Team: 1, // Enemy
                    X: enemyStartX,
                    Z: (i - 1) * spacingZ,
                    Hp: cdef.BaseHp,
                    MaxHp: cdef.BaseHp,
                    ModelCode: $"Enemy_{cid}"
                ));
            }

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
            var pack = await _md.BuildPackAsync(req.StageId, partyIds, ct);

            // 5) Aggregate 생성
            var combat = Domain.Entities.Combat.Create(
                Domain.Enum.CombatMode.Pve, req.StageId, seed, input,
                balanceVersion: "1", // TODO: 운영툴/설정에서 주입
                clientVersion: req.ClientVersion);

            // 6) 전투 시뮬레이션
            var result = _engine.Simulate(input, seed, pack);

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
        private static (float x, float z) GetPlayerPositionBySlot(int slotId)
        {
            // 예시: 0~4 앞줄, 5~9 뒷줄, 왼쪽에서 오른쪽으로
            const float baseX = -3.0f;
            const float stepX = 1.2f;
            const float frontZ = 0.5f;
            const float backZ = -0.5f;

            var col = slotId % 5;   // 0~4
            var row = slotId / 5;   // 0 or 1

            var x = baseX + col * stepX;
            var z = row == 0 ? frontZ : backZ;
            return (x, z);
        }
    }
}
