using Application.Combat.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Combat.Engine
{
    public interface ICombatTickEngine
    {
        List<CombatLogEventDto> Process(CombatRuntimeState state);
        CombatSnapshotDto BuildSnapshot(CombatRuntimeState s, List<CombatLogEventDto> events);


    }
}
