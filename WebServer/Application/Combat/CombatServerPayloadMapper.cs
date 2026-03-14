using System.Net.Http.Json;
using Application.Combat.Dto;
using Domain.Combat.Engine;
using Domain.Entities.Combats;

namespace Application.Combat
{  
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
