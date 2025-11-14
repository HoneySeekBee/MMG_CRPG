using Application.Combat.Engine.TickSystems;
using Application.Combat.Runtime;
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
        private readonly WaveSystem _wave = new(); 
        private readonly SnapshotBuilder _snapshot = new SnapshotBuilder(); 
        public List<CombatLogEventDto> Process(CombatRuntimeState state)
        {
            var events = new List<CombatLogEventDto>();
            if (state.BattleEnded)
                return events;

            _commands.Run(state, events);
            _ai.Run(state, events);
            _move.Run(state, events);
            _atk.Run(state, events);
            _death.Run(state, events);
            _wave.Run(state, events);

            return events;
        }

        public CombatSnapshotDto BuildSnapshot(CombatRuntimeState s)
            => _snapshot.Build(s);
    }
}
