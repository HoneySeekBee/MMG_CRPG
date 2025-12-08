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
        public int HpMax { get; set; }
        public bool Dead { get; set; }
        public bool ReturningToSpawn { get; set; }

        // Base Stat ( 변하지 않는 스텟 )
        public int AtkBase { get; set; }
        public int DefBase { get; set; }
        public int SpdBase { get; set; }
        public float RangeBase { get; set; }
        public int AttackIntervalMsBase { get; set; }
        public double CritRateBase { get; set; }
        public double CritDamageBase { get; set; }

        // Effective Stat ( 버프 등 반영된 최종값 ) 
        public int AtkEff { get; set; }
        public int DefEff { get; set; }
        public int SpdEff { get; set; }
        public double CritRateEff { get; set; }
        public double CritDamageEff { get; set; }

        public float RangeEff { get; set; }
        public int AttackCooldownMs { get; set; }
        public int SkillCooldownMs { get; set; }

        public long? TargetActorId { get; set; }
        public int Waveindex { get; set; }
         
        public List<AppliedBuff> Buffs { get; } = new();
    }
}
