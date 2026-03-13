using Domain.Combat.Engine.TickSystems;
using Domain.Combat.Engine.TickSystems.Skill;
using Domain.Combat.Runtime;
using Domain.Entities.Combats;
using Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Combat.Engine
{
    public sealed class CombatTickEngine : ICombatTickEngine
    {
        private readonly PlayerCommandSystem _commands = new();
        private readonly AiSystem _ai = new();
        private readonly MovementSystem _move = new();
        private readonly AttackSystem _atk = new();
        private readonly DeathSystem _death = new();
        private readonly SkillSystem _skill = new();
        private readonly WaveSystem _wave = new();
        private readonly SnapshotBuilder _snapshot = new SnapshotBuilder();
        private readonly BuffTickSystem _buffTicks = new();
        private readonly BuffStatSystem _buffStats = new();
        private readonly CrowdControlSystem _cc = new();
        private readonly ProjectileSystem _proj = new();
        public List<CombatLogEvent> Process(CombatRuntimeState state, int dtMs)
        {
            var events = new List<CombatLogEvent>();
            if (state.BattleEnded)
                return events;

            dtMs = Math.Min(dtMs, 200);

            state.Tick++;
            state.SimTimeMs += dtMs;

            _commands.Run(state, events);
            _ai.Run(state, events);

            _cc.Run(state, dtMs);
            _move.Run(state, events, dtMs);

            _buffTicks.Run(state, events, dtMs);
            _buffStats.Run(state);

            _skill.Run(state, events, dtMs);
            _proj.Run(state, events, dtMs);
            _atk.Run(state, events, dtMs);
            _death.Run(state, events);
            _wave.Run(state, events);

            return events;
        }


        public CombatSnapshotDto BuildSnapshot(CombatRuntimeState s)
        {
            CombatSnapshot domainSnap = _snapshot.Build(s);
            return CombatSnapshotMapper.ToDto(domainSnap);
        }
    }
}
