using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Combat.Runtime
{
    public sealed class CombatRuntimeSnapshot
    {
        public Dictionary<long, ActorState> Actors { get; } = new();
    }
}
