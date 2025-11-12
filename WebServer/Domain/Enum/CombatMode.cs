using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enum
{
    public enum CombatMode
    {
        Pve = 0,
        Pvp = 1
    }
    public enum CombatResult
    {
        Unknown = 0,
        Win = 1,
        Lose = 2,
        Error = 3
    }
}
