using Domain.Combat.Runtime;
using Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Combat.Engine
{
    public interface ICombatTickEngine
    {
        List<CombatLogEvent> Process(CombatRuntimeState state, int dtMs);
        CombatSnapshotDto BuildSnapshot(CombatRuntimeState s);


    }
}
