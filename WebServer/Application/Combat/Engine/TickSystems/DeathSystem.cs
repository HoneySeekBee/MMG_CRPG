using Application.Combat.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Combat.Engine.TickSystems
{
    public sealed class DeathSystem
    {
        public void Run(CombatRuntimeState s, List<CombatLogEventDto> evs)
        {
            // 1) HP 0된 애들 Dead 플래그 및 이벤트
            foreach (var a in s.ActiveActors.Values)
            {
                if (a.Hp <= 0 && !a.Dead)
                {
                    a.Hp = 0;           //  음수 방지
                    a.Dead = true;

                    evs.Add(new CombatLogEventDto(
                        TMs: NowMs(s),
                        Type: "dead",
                        Actor: a.ActorId.ToString(),
                        Target: null,
                        Damage: null,
                        Crit: null,
                        Extra: null
                    ));
                }
            }

            // 여기서부터는 전투 종료 판정
            if (s.BattleEnded)
                return;

            var alive = s.ActiveActors.Values
                .Where(a => !a.Dead && a.Hp > 0)
                .ToList();

            if (!alive.Any())
                return;

            bool anyPlayerAlive = alive.Any(a => a.Team == 0);
            bool anyEnemyAlive = alive.Any(a => a.Team == 1);

            //  적은 다 죽었더라도, 다음 웨이브가 있을 수 있으니
            //    "승리 처리"는 WaveSystem에 맡기고 여기서는 패배만 본다.
            if (!anyPlayerAlive)
            {
                s.BattleEnded = true;

                evs.Add(new CombatLogEventDto(
                    TMs: NowMs(s),
                    Type: "stage_result",
                    Actor: "",
                    Target: "",
                    Damage: null,
                    Crit: null,
                    Extra: new Dictionary<string, object?>
                    {
                        ["result"] = "lose"
                    }
                ));
            }

            // anyPlayerAlive && !anyEnemyAlive 인 경우:
            // → WaveSystem.Run 이 wave_cleared / stage_cleared 이벤트를 찍고
            //   최종 승리 처리까지 담당한다.
        }


        private int NowMs(CombatRuntimeState s)
            => (int)(DateTimeOffset.UtcNow - s.StartedAt).TotalMilliseconds;
    }
}
