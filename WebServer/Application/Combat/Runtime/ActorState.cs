using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Combat.Runtime
{
    public sealed class ActorState
    {
        public long ActorId { get; init; }
        public int Team { get; init; }
        public float X { get; set; }
        public float Z { get; set; }
        public float SpawnX { get; set; }
        public float SpawnZ { get; set; }
        public int Hp { get; set; }
        public bool Dead { get; set; }
        public bool ReturningToSpawn { get; set; }

        // 전투 스텟 
        public int Atk { get; set; }
        public int Def { get; set; }
        public int Spd { get; set; }       
        public float Range { get; set; }
        public int AttackIntervalMs { get; set; }
        public double CritRate { get; set; }    // 0.10 이런 식
        public double CritDamage { get; set; }  // 0.50 = +50%


        public int AttackCooldownMs { get; set; }
        public int SkillCooldownMs { get; set; }

        public long? TargetActorId { get; set; }
        public int Waveindex { get; set; }
    }
}
