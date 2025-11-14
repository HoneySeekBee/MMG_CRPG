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
                1 => (13.5f, 2.6f),
                2 => (13.5f, 0f),
                3 => (13.5f, -2.43f),
                4 => (15.5f, 2.6f),
                5 => (15.5f, 0f),
                6 => (15.5f, -2.43f),
                7 => (17.5f, 2.6f),
                8 => (17.5f, 0f),
                9 => (17.5f, -2.43f),
                _ => (5f, 0f)
            };
        }

        public static (float x, float z) GetPlayerPositionBySlot(int slot)
        {
            // Unity에서 쓰는 좌표 그대로 옮긴 버전
            return slot switch
            {
                1 => (0f, -2.59f),
                2 => (0f, 0.33f),
                3 => (0f, 2.42f),

                4 => (-2f, -2.59f),
                5 => (-2f, 0.33f),
                6 => (-2f, 2.42f),

                7 => (-4f, -2.59f),
                8 => (-4f, 0.33f),
                9 => (-4f, 2.42f),

                _ => (0f, 0f) // 안전용 디폴트
            };
        }
    }
}
