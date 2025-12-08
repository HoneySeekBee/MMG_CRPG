using Application.Combat.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Combat.Engine.TickSystems
{
    public sealed class PlayerCommandSystem
    {
        public void Run(CombatRuntimeState state, List<CombatLogEventDto> evs)
        {
            while (state.PendingCommands.Count > 0)
            {
                var cmd = state.PendingCommands.Dequeue();

                if (!state.ActiveActors.TryGetValue(cmd.ActorId, out var actor))
                    continue;

                if (actor.SkillCooldownMs > 0)
                    continue;

                // 쿨 적용
                actor.SkillCooldownMs = 3000;

                // 스킬 처리 큐로 보내기
                state.PendingSkillCasts.Enqueue(new PendingSkillCast
                {
                    CasterId = cmd.ActorId,
                    TargetId = cmd.TargetActorId,
                    SkillId = cmd.SkillId,
                    SkillLevel = cmd.SkillLevel,
                });

                evs.Add(new CombatLogEventDto(
                    NowMs(state),
                    "skill_cast",
                    actor.ActorId.ToString(),
                    cmd.TargetActorId?.ToString(),
                    null,
                    null,
                    new Dictionary<string, object?> { ["skillId"] = cmd.SkillId }
                ));
            }
        }

        private int NowMs(CombatRuntimeState s)
            => (int)(DateTimeOffset.UtcNow - s.StartedAt).TotalMilliseconds;
    }
}
