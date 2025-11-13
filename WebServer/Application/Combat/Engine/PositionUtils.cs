using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Combat.Engine
{
    public static class PositionUtils
    {
        public static (float x, float z) GetEnemyPositionBySlot(int slot)
        { 
            return slot switch
            {
                1 => (3f, 1f),
                2 => (4f, 0f),
                3 => (3f, -1f),
                _ => (5f, 0f)
            };
        }

        public static (float x, float z) GetPlayerPositionBySlot(int slot)
        {
            return slot switch
            {
                1 => (-3f, 1f),
                2 => (-4f, 0f),
                3 => (-3f, -1f),
                _ => (-5f, 0f)
            };
        }
    }
}
