using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Monsters
{
    public class MonsterStatDto
    {
        public int MonsterId { get; set; }
        public int Level { get; set; }
        public int HP { get; set; }
        public int ATK { get; set; }
        public int DEF { get; set; }
        public int SPD { get; set; }
        public decimal CritRate { get; set; }
        public decimal CritDamage { get; set; }
        public float Range { get; set; }
    }
}
