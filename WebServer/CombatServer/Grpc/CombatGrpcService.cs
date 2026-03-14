using Application.Combat;
using CombatInternal;
using Grpc.Core;

namespace CombatServer.Grpc
{
    public class CombatGrpcService : CombatInternalService.CombatInternalServiceBase
    {
        private readonly ICombatService _combat;

        public CombatGrpcService(ICombatService combat)
        {
            _combat = combat;
        }

        public override async Task<InitCombatResponse> InitCombat(InitCombatRequest request, ServerCallContext context)
        {
            // InitCombatRequest → InitCombatPayload 변환
            var payload = new InitCombatPayload
            {
                CombatId = request.CombatId,
                StageId = request.StageId,
                UserId = request.UserId,
                Seed = request.Seed,
                Players = request.Players.Select(p => new PlayerSlotPayload
                {
                    SlotId = p.SlotId,
                    CharacterId = p.CharacterId,
                    Hp = p.Hp
                }).ToList(),
                Stage = new StageDefPayload
                {
                    StageId = request.Stage.StageId,
                    Waves = request.Stage.Waves.Select(w => new WaveDefPayload
                    {
                        Index = w.Index,
                        Enemies = w.Enemies.Select(e => new EnemySpawnPayload
                        {
                            Slot = e.Slot,
                            MonsterId = e.MonsterId,
                            Level = e.Level
                        }).ToList()
                    }).ToList()
                },
                ActorDefs = request.ActorDefs.ToDictionary(
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
                    })
            };

            var snapshot = await _combat.InitCombatAsync(payload, context.CancellationToken);

            var response = new InitCombatResponse();
            response.Actors.AddRange(snapshot.Actors.Select(a => new GrpcActorInit
            {
                ActorId = a.ActorId,
                Team = a.Team,
                X = a.X,
                Z = a.Z,
                Hp = a.Hp,
                WaveIndex = a.WaveIndex,
                MasterId = a.MasterId
            }));

            return response;
        }
        public override async Task<GetCombatResultResponse> GetCombatResult(
          GetCombatResultRequest request, ServerCallContext context)
        {
            var result = await _combat.GetResultAsync(request.CombatId, context.CancellationToken);

            return new GetCombatResultResponse
            {
                CombatId = result.CombatId,
                StageId = result.StageId,
                UserId = result.UserId,
                BattleEnded = result.BattleEnded,
                Result = (int)(result.Result ?? 0),
                DeadPlayerCount = result.DeadPlayerCount,
                TotalPlayerCount = result.TotalPlayerCount
            };
        }

    }
}
