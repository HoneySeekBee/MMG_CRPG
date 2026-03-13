using System.Net.Http.Json;
using Domain.Combat.Engine;
using Domain.Entities.Combats;

namespace Application.Combat
{
    // Calls CombatServer HTTP endpoints on behalf of WebServer
    public sealed class CombatServerClient
    {
        private readonly HttpClient _http;

        public CombatServerClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<CombatInitialSnapshotPayload> InitCombatAsync(InitCombatPayload payload, CancellationToken ct)
        {
            var response = await _http.PostAsJsonAsync("combat/init", payload, ct);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<CombatInitialSnapshotPayload>(cancellationToken: ct);
            return result ?? throw new InvalidOperationException("Empty response from CombatServer");
        }

        public async Task<CombatResultPayload> GetResultAsync(long combatId, CancellationToken ct)
        {
            var response = await _http.GetAsync($"combat/{combatId}/result", ct);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<CombatResultPayload>(cancellationToken: ct);
            return result ?? throw new InvalidOperationException("Empty response from CombatServer");
        }
    }

    // DTOs matching CombatServer's response shapes

    public sealed class CombatInitialSnapshotPayload
    {
        public List<ActorInitPayload> Actors { get; init; } = new();
    }

    public sealed class ActorInitPayload
    {
        public long ActorId { get; init; }
        public int Team { get; init; }
        public float X { get; init; }
        public float Z { get; init; }
        public int Hp { get; init; }
        public int WaveIndex { get; init; }
        public long MasterId { get; init; }
    }

    public sealed record CombatResultPayload(
        long CombatId,
        int StageId,
        long UserId,
        bool BattleEnded,
        Domain.Enum.CombatResult? Result,
        int DeadPlayerCount,
        int TotalPlayerCount
    );

    // Payload WebServer sends to CombatServer /combat/init

    public sealed class InitCombatPayload
    {
        public long CombatId { get; init; }
        public int StageId { get; init; }
        public long UserId { get; init; }
        public long Seed { get; init; }
        public List<PlayerSlotPayload> Players { get; init; } = new();
        public StageDefPayload Stage { get; init; } = null!;
        public Dictionary<long, ActorDefPayload> ActorDefs { get; init; } = new();
    }

    public sealed class PlayerSlotPayload
    {
        public int SlotId { get; init; }
        public long CharacterId { get; init; }
        public int Hp { get; init; }
    }

    public sealed class StageDefPayload
    {
        public int StageId { get; init; }
        public List<WaveDefPayload> Waves { get; init; } = new();
    }

    public sealed class WaveDefPayload
    {
        public int Index { get; init; }
        public List<EnemySpawnPayload> Enemies { get; init; } = new();
    }

    public sealed class EnemySpawnPayload
    {
        public int Slot { get; init; }
        public int MonsterId { get; init; }
        public int Level { get; init; }
    }

    public sealed class ActorDefPayload
    {
        public int MasterId { get; init; }
        public bool IsPlayer { get; init; }
        public string ModelKey { get; init; } = "";
        public int MaxHp { get; init; }
        public int Atk { get; init; }
        public int Def { get; init; }
        public int Spd { get; init; }
        public float Range { get; init; }
        public int AttackIntervalMs { get; init; }
        public double CritRate { get; init; }
        public double CritDamage { get; init; }
    }

    // Maps Domain models → CombatServer payload
    public static class CombatServerPayloadMapper
    {
        public static InitCombatPayload Build(
            long combatId,
            int stageId,
            long userId,
            long seed,
            IEnumerable<(int SlotId, long CharacterId, int Hp)> players,
            MasterPack pack)
        {
            var stageDef = new StageDefPayload
            {
                StageId = pack.Stage.StageId,
                Waves = pack.Stage.Waves.Select(w => new WaveDefPayload
                {
                    Index = w.Index,
                    Enemies = w.Enemies.Select(e => new EnemySpawnPayload
                    {
                        Slot = e.Slot,
                        MonsterId = e.MonsterId,
                        Level = e.Level
                    }).ToList()
                }).ToList()
            };

            var actorDefs = pack.Actors.ToDictionary(
                kvp => kvp.Key,
                kvp => new ActorDefPayload
                {
                    MasterId = kvp.Value.MasterId,
                    IsPlayer = kvp.Value.IsPlayer,
                    ModelKey = kvp.Value.ModelKey,
                    MaxHp = kvp.Value.MaxHp,
                    Atk = kvp.Value.Atk,
                    Def = kvp.Value.Def,
                    Spd = kvp.Value.Spd,
                    Range = kvp.Value.Range,
                    AttackIntervalMs = kvp.Value.AttackIntervalMs,
                    CritRate = kvp.Value.CritRate,
                    CritDamage = kvp.Value.CritDamage
                });

            return new InitCombatPayload
            {
                CombatId = combatId,
                StageId = stageId,
                UserId = userId,
                Seed = seed,
                Players = players.Select(p => new PlayerSlotPayload
                {
                    SlotId = p.SlotId,
                    CharacterId = p.CharacterId,
                    Hp = p.Hp
                }).ToList(),
                Stage = stageDef,
                ActorDefs = actorDefs
            };
        }
    }
}
