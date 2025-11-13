using Application.Monsters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Combat.Engine
{
    public interface IMonsterStatReader 
    { 
        Task<MonsterStatDto?> GetAsync(long monsterId, int level, CancellationToken ct);
    
    }
}
