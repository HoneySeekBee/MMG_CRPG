using Application.Combat;
using Application.Combat.Dto;
using CombatInternal;
using Grpc.Net.Client;
using Infrastructure.Services.Combat;

namespace Infrastructure.Services.Grpc;

public sealed class GrpcCombatServerClient : ICombatServerClient
{
    private readonly CombatServerSelector _selector;
    private readonly CombatRouteStore _routeStore;

    public GrpcCombatServerClient(CombatServerSelector selector, CombatRouteStore routeStore)
    {
        _selector = selector;
        _routeStore = routeStore;
    }

    public async Task<CombatInitialSnapshotPayload> InitCombatAsync(InitCombatPayload payload, CancellationToken ct)
    {
        // Select a live CombatServer and persist the route so finish can find it later
        var serverUrl = _selector.SelectServer();
        await _routeStore.SaveAsync(payload.CombatId, serverUrl, ct);

        var client = CreateClient(serverUrl);

        var request = new InitCombatRequest
        {
            CombatId = payload.CombatId,
            StageId = payload.StageId,
            UserId = payload.UserId,
            Seed = payload.Seed,
            Stage = new GrpcStageDef
            {
                StageId = payload.Stage.StageId,
                Waves = { payload.Stage.Waves.Select(w => new GrpcWaveDef
                  {
                      Index   = w.Index,
                      Enemies = { w.Enemies.Select(e => new GrpcEnemySpawn
                      {
                          Slot      = e.Slot,
                          MonsterId = e.MonsterId,
                          Level     = e.Level
                      })}
                  })}
            }
        };

        request.Players.AddRange(payload.Players.Select(p => new GrpcPlayerSlot
        {
            SlotId = p.SlotId,
            CharacterId = p.CharacterId,
            Hp = p.Hp
        }));

        request.ActorDefs.Add(payload.ActorDefs.ToDictionary(
            kvp => kvp.Key,
            kvp => new GrpcActorDef
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
            }));

        var response = await client.InitCombatAsync(request, cancellationToken: ct);

        return new CombatInitialSnapshotPayload
        {
            Actors = response.Actors.Select(a => new ActorInitPayload
            {
                ActorId = a.ActorId,
                Team = a.Team,
                X = a.X,
                Z = a.Z,
                Hp = a.Hp,
                WaveIndex = a.WaveIndex,
                MasterId = a.MasterId
            }).ToList()
        };
    }

    public async Task<CombatResultPayload> GetResultAsync(long combatId, CancellationToken ct)
    {
        var serverUrl = await _routeStore.GetAsync(combatId, ct)
            ?? throw new InvalidOperationException($"No route found for combat {combatId}.");

        var client = CreateClient(serverUrl);

        var response = await client.GetCombatResultAsync(
            new GetCombatResultRequest { CombatId = combatId },
            cancellationToken: ct);

        return new CombatResultPayload(
            CombatId: response.CombatId,
            StageId: response.StageId,
            UserId: response.UserId,
            BattleEnded: response.BattleEnded,
            Result: (Domain.Enum.CombatResult?)response.Result,
            DeadPlayerCount: response.DeadPlayerCount,
            TotalPlayerCount: response.TotalPlayerCount
        );
    }

    private static CombatInternalService.CombatInternalServiceClient CreateClient(string serverUrl)
    {
        // Use GrpcChannel with HTTP/2 clear-text (h2c) for internal Docker network calls
        var channel = GrpcChannel.ForAddress(serverUrl, new GrpcChannelOptions
        {
            HttpHandler = new HttpClientHandler()
        });
        return new CombatInternalService.CombatInternalServiceClient(channel);
    }
}
