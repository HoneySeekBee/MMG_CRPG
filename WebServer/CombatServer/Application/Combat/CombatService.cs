using Application.Combat.Engine;
using Domain.Combat.Runtime;
using Domain.Entities.Combats;
using Application.Repositories;
using Application.Skills;
using Application.UserCharacter;
using Domain.Enum;
using System.Collections.Concurrent;
using Domain.Combat.Engine;
using CombatEntity = Domain.Entities.Combats.Combat;

namespace Application.Combat
{
    public sealed class CombatService : ICombatService
    {
        private readonly IMasterDataProvider _master;
        private readonly ICombatRepository _repo;
        private readonly ICombatTickEngine _tickEngine;
        private readonly ISkillCache _skillCache;
        private readonly IUserCharacterReader _userCharacterReader;

        private const int MaxPageSize = 500;
        private static readonly ConcurrentDictionary<long, CombatRuntimeState> _runtimeStates = new();

        public CombatService(
            IMasterDataProvider master,
            ICombatRepository repo,
            ICombatTickEngine tickEngine,
            ISkillCache skillCache,
            IUserCharacterReader userCharacterReader)
        {
            _master = master;
            _repo = repo;
            _tickEngine = tickEngine;
            _skillCache = skillCache;
            _userCharacterReader = userCharacterReader;
        }

        public async Task<CombatInitialSnapshotDto> InitCombatAsync(InitCombatPayload payload, CancellationToken ct)
        {
            // (1) Rebuild CombatMasterDataPack from payload
            var waveDefs = payload.Stage.Waves.Select(w =>
                new CombatWaveDef(w.Index, w.Enemies.Select(e =>
                    new CombatEnemySpawn(e.Slot, e.MonsterId, e.Level)
                ).ToList())
            ).ToList();

            var stageDef = new CombatStageDef(payload.Stage.StageId, waveDefs);

            var actorDefs = payload.ActorDefs.ToDictionary(
                kvp => kvp.Key,
                kvp => new CombatActorDef(
                    kvp.Value.MasterId, kvp.Value.IsPlayer, kvp.Value.ModelKey,
                    kvp.Value.MaxHp, kvp.Value.Atk, kvp.Value.Def, kvp.Value.Spd,
                    kvp.Value.Range, kvp.Value.AttackIntervalMs,
                    kvp.Value.CritRate, kvp.Value.CritDamage));

            var pack = new MasterPack(stageDef, actorDefs);

            // (2) Initialize runtime state
            _runtimeStates[payload.CombatId] = new CombatRuntimeState
            {
                CombatId = payload.CombatId,
                StageId = payload.StageId,
                UserId = (int)payload.UserId,
                Seed = payload.Seed,
                StartedAt = DateTimeOffset.UtcNow,
                CurrentWaveIndex = 1,
                TotalWaves = pack.Stage.Waves.Count
            };

            var state = _runtimeStates[payload.CombatId];
            state.Snapshot = new CombatRuntimeSnapshot();
            state._MasterPack = pack;

            var skills = _skillCache.GetAll();
            state.SkillMaster.Clear();
            foreach (var s in skills)
                state.SkillMaster[s.SkillId] = CombatSkillMapper.ToCombatSkill(s);

            // (3) Build actor list
            var actors = new List<ActorInitDto>();

            foreach (var player in payload.Players)
            {
                var (x, z) = PositionUtils.GetPlayerPositionBySlot(player.SlotId);
                var actorId = 1 + player.SlotId;
                actors.Add(new ActorInitDto(actorId, 0, x, z, player.Hp, 1, player.CharacterId));
            }

            foreach (var wave in pack.Stage.Waves)
            {
                foreach (var spawn in wave.Enemies)
                {
                    var (x, z) = PositionUtils.GetEnemyPositionBySlot(spawn.Slot);
                    var actorId = 1000 * wave.Index + spawn.Slot;
                    long cid = spawn.MonsterId;
                    var cdef = pack.Actors[cid];
                    actors.Add(new ActorInitDto(actorId, 1, x, z, cdef.MaxHp, wave.Index, cid));
                }
            }

            // (4) Load actors into snapshot
            foreach (var a in actors)
            {
                var def = pack.Actors[a.MasterId];
                state.Snapshot.Actors[a.ActorId] = new ActorState
                {
                    ActorId = a.ActorId, Team = a.Team,
                    X = a.X, Z = a.Z, SpawnX = a.X, SpawnZ = a.Z,
                    Hp = a.Hp,
                    AtkBase = def.Atk, DefBase = def.Def, SpdBase = def.Spd,
                    RangeBase = def.Range, AttackIntervalMsBase = def.AttackIntervalMs,
                    CritRateBase = def.CritRate, CritDamageBase = def.CritDamage,
                    AttackCooldownMs = 0, SkillCooldownMs = 0,
                    TargetActorId = null, Waveindex = a.WaveIndex
                };
            }

            // (5) Activate first wave
            state.ActiveActors.Clear();
            foreach (var a in state.Snapshot.Actors.Values)
            {
                if (a.Waveindex == state.CurrentWaveIndex && a.Hp > 0)
                    state.ActiveActors[a.ActorId] = a;
            }

            // (6) Persist combat record
            var combatInput = new CombatInputSnapshot(
                payload.StageId,
                payload.Players.Select(p => new PartyMember(p.CharacterId, 1)).ToArray(),
                Array.Empty<SkillInput>());

            var combat = CombatEntity.Create(
                CombatMode.Pve, payload.StageId, payload.Seed,
                combatInput, balanceVersion: "1", clientVersion: null);

            await _repo.SaveAsync(combat, Enumerable.Empty<Domain.Events.CombatLogEvent>(), ct);

            return new CombatInitialSnapshotDto(actors);
        }

        public Task<CombatResultPayload> GetResultAsync(long combatId, CancellationToken ct)
        {
            if (!_runtimeStates.TryGetValue(combatId, out var state))
                throw new KeyNotFoundException($"Combat {combatId} not found");

            if (!state.BattleEnded)
                throw new InvalidOperationException("COMBAT_NOT_FINISHED");

            var players = state.Snapshot.Actors.Values.Where(a => a.Team == 0).ToList();
            int deadCount = players.Count(a => a.Dead || a.Hp <= 0);

            var result = new CombatResultPayload(
                CombatId: combatId,
                StageId: state.StageId,
                UserId: state.UserId,
                BattleEnded: state.BattleEnded,
                Result: state.Result,
                DeadPlayerCount: deadCount,
                TotalPlayerCount: players.Count);

            // Remove from memory after result is retrieved
            _runtimeStates.TryRemove(combatId, out _);

            return Task.FromResult(result);
        }

        public async Task<StartCombatResponse> StartAsync(StartCombatRequest req, CancellationToken ct)
        {
            if (req.StageId <= 0)
                throw new ArgumentException("StageId must be positive.", nameof(req.StageId));

            var partyCharacterIds = req.PartyCharacterIds
                ?? throw new ArgumentNullException(nameof(req.PartyCharacterIds));

            // (1) Load user character stats
            var userCharStats = await _userCharacterReader.GetManyByCharacterIdAsync(partyCharacterIds, req.UserId, ct);
            var statsByCharacterId = userCharStats.ToDictionary(x => (long)x.CharacterId);

            // (2) Build master data pack
            var pack = await _master.BuildPackAsync(req.StageId, req.UserId, partyCharacterIds, ct);

            // (3) Build party members
            var partyMembers = partyCharacterIds.Select(charId =>
            {
                if (!statsByCharacterId.TryGetValue(charId, out var uc))
                    throw new InvalidOperationException($"CharacterId {charId} not found in stats.");
                return new PartyMember(CharacterId: uc.CharacterId, Level: uc.Level);
            }).ToArray();

            var input = new CombatInputSnapshot(req.StageId, partyMembers, Array.Empty<SkillInput>());

            var seed = BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 0);
            if (seed == 0) seed = 1;

            // (4) Create and persist combat aggregate
            var combat = CombatEntity.Create(
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

            // (5) Initialize runtime state
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
            runtimeState._MasterPack = pack;

            var skills = _skillCache.GetAll();
            runtimeState.SkillMaster.Clear();
            foreach (var s in skills)
            {
                runtimeState.SkillMaster[s.SkillId] = CombatSkillMapper.ToCombatSkill(s);
            }

            // (6) Build actor list
            var actors = new List<ActorInitDto>();

            int slotId = 0;
            foreach (var charId in partyCharacterIds)
            {
                var uc = statsByCharacterId[charId];
                var (x, z) = PositionUtils.GetPlayerPositionBySlot(slotId);
                var actorId = 1 + slotId;
                actors.Add(new ActorInitDto(
                    ActorId: actorId,
                    Team: 0,
                    X: x,
                    Z: z,
                    Hp: uc.Hp,
                    WaveIndex: 1,
                    MasterId: uc.CharacterId
                ));
                slotId++;
            }

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

            // (7) Load actors into snapshot
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
                    AtkBase = def.Atk,
                    DefBase = def.Def,
                    SpdBase = def.Spd,
                    RangeBase = def.Range,
                    AttackIntervalMsBase = def.AttackIntervalMs,
                    CritRateBase = def.CritRate,
                    CritDamageBase = def.CritDamage,
                    AttackCooldownMs = 0,
                    SkillCooldownMs = 0,
                    TargetActorId = null,
                    Waveindex = a.WaveIndex
                };
            }

            // (8) Activate first wave actors
            runtimeState.ActiveActors.Clear();
            foreach (var a in runtimeState.Snapshot.Actors.Values)
            {
                if (a.Waveindex == runtimeState.CurrentWaveIndex && a.Hp > 0)
                    runtimeState.ActiveActors[a.ActorId] = a;
            }

            var snapshot = new CombatInitialSnapshotDto(actors);
            return new StartCombatResponse(combatId, snapshot);
        }

        public async Task EnqueueCommandAsync(long combatId, CombatCommandDto cmd, CancellationToken ct)
        {
            if (!_runtimeStates.TryGetValue(combatId, out var state))
                throw new KeyNotFoundException($"Combat runtime state not found for id {combatId}");

            lock (state.SyncRoot)
            {
                state.PendingCommands.Enqueue(new CombatCommand(
                    cmd.ActorId,
                    cmd.TargetActorId,
                    cmd.SkillId,
                    cmd.SkillLevel
                ));
            }

            var tMs = (int)(DateTimeOffset.UtcNow - state.StartedAt).TotalMilliseconds;
            var ev = new Domain.Events.CombatLogEvent(
                TMs: tMs,
                Type: "skill_used",
                Actor: cmd.ActorId.ToString(),
                Target: cmd.TargetActorId?.ToString(),
                Damage: null,
                Crit: null,
                Extra: new Dictionary<string, object?> { ["skillId"] = cmd.SkillId }
            );

            await _repo.AppendLogsAsync(combatId, new[] { ev }, ct);
        }

        public async Task<CombatTickResponse> TickAsync(long combatId, int tick, CancellationToken ct)
        {
            if (!_runtimeStates.TryGetValue(combatId, out var state))
                throw new KeyNotFoundException($"Combat {combatId} not found");

            List<Domain.Events.CombatLogEvent> domainEvs = new();
            CombatSnapshotDto snapshot;

            const int BaseTickMs = 100;
            const int MaxCatchUpTicks = 5;

            lock (state.SyncRoot)
            {
                if (tick <= state.Tick)
                {
                    snapshot = _tickEngine.BuildSnapshot(state);
                    return new CombatTickResponse(combatId, state.Tick, snapshot, new List<CombatLogEventDto>());
                }

                int missing = tick - state.Tick;
                int catchUp = Math.Min(missing, MaxCatchUpTicks);
                int tickDeltaMs = (int)(BaseTickMs * state.TimeScale);

                for (int i = 0; i < catchUp; i++)
                {
                    var step = _tickEngine.Process(state, tickDeltaMs);
                    if (step.Count > 0) domainEvs.AddRange(step);
                }

                snapshot = _tickEngine.BuildSnapshot(state);
            }

            if (domainEvs.Count > 0)
                await _repo.AppendLogsAsync(combatId, domainEvs, ct);

            var dtoEvs = domainEvs.Select(Map).ToList();
            return new CombatTickResponse(combatId, state.Tick, snapshot, dtoEvs);
        }

        public Task<CombatSpeed> ToggleSpeedAsync(long combatId, CancellationToken ct)
        {
            if (!_runtimeStates.TryGetValue(combatId, out var state))
                throw new KeyNotFoundException($"Combat {combatId} not found");

            lock (state.SyncRoot)
            {
                state.Speed = state.Speed switch
                {
                    CombatSpeed.X1 => CombatSpeed.X15,
                    CombatSpeed.X15 => CombatSpeed.X2,
                    _ => CombatSpeed.X1
                };

                return Task.FromResult(state.Speed);
            }
        }

        public async Task<CombatLogPageDto> GetLogAsync(long combatId, string? cursor, int size, CancellationToken ct)
        {
            if (size <= 0) size = 100;
            size = Math.Min(size, MaxPageSize);
            return await _repo.GetLogAsync(combatId, cursor, size, ct);
        }

        public Task<CombatLogSummaryDto> GetSummaryAsync(long combatId, CancellationToken ct)
            => _repo.GetSummaryAsync(combatId, ct);

        private static CombatLogEventDto Map(Domain.Events.CombatLogEvent e)
            => new(e.TMs, e.Type, e.Actor, e.Target, e.Damage, e.Crit, e.Extra);
    }
}
