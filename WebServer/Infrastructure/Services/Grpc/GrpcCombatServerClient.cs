using Application.Combat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CombatInternal;
using Application.Combat.Dto;

namespace Infrastructure.Services.Grpc;

public sealed class GrpcCombatServerClient : ICombatServerClient
{
    private readonly CombatInternalService.CombatInternalServiceClient _grpc;

    public GrpcCombatServerClient(CombatInternalService.CombatInternalServiceClient grpc)
    {
        _grpc = grpc;
    }
    public async Task<CombatInitialSnapshotPayload> InitCombatAsync(InitCombatPayload payload, CancellationToken ct)
    {
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

        var response = await _grpc.InitCombatAsync(request, cancellationToken: ct);

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
        var response = await _grpc.GetCombatResultAsync(
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
}
