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
        public bool ArrivedAtSpawn { get; set; }

        // Base Stat ( 변하지 않는 스텟 )
        public int AtkBase { get; set; }
        public int DefBase { get; set; }
        public int SpdBase { get; set; }
        public float RangeBase { get; set; }
        public int AttackIntervalMsBase { get; set; }
        public double CritRateBase { get; set; }
        public double CritDamageBase { get; set; }

        // Effective Stat ( 버프 등 반영된 최종값 ) 
        public int AtkEff => AtkBase + BuffAtk;
        public int DefEff => DefBase + BuffDef;
        public int SpdEff { get; set; }

        public double CritRateEff => CritRateBase + BuffCritRate;
        public double CritDamageEff => CritDamageBase + BuffCritDamage;

        public float RangeEff { get; set; }
        public int AttackCooldownMs { get; set; }
        public int SkillCooldownMs { get; set; }

        public long? TargetActorId { get; set; }
        public int Waveindex { get; set; }
         
        public List<AppliedBuff> Buffs { get; } = new();
        public float FacingX { get; set; } = 0f;
        public float FacingZ { get; set; } = 1f;
        
        // CC 상태
        public bool Stunned { get; set; }
        public bool Silenced { get; set; }
        public bool Frozen { get; set; }
        public bool Rooted { get; set; }
        public bool KnockedDown { get; set; }

        // CC 지속시간
        public int StunMs { get; set; }
        public int SilenceMs { get; set; }
        public int FreezeMs { get; set; }
        public int RootMs { get; set; }
        public int KnockdownMs { get; set; }

        public bool IsKnockbacked { get; set; }
        public float KnockbackVX { get; set; }
        public float KnockbackVZ { get; set; }
        public int KnockbackRemainMs { get; set; }
        public int Shield { get; set; } = 0;
        public int ShieldMax { get; set; } = 0;

        public bool ImmuneStun { get; set; }
        public bool ImmuneSilence { get; set; }
        public bool ImmuneFreeze { get; set; }
        public bool ImmuneRoot { get; set; }
        public bool ImmuneKnockdown { get; set; }
        public bool ImmuneKnockback { get; set; }
        public bool ImmuneDebuff { get; set; }

        // 저항
        public float StunResistChance { get; set; }     // 0~1
        public float FreezeResistChance { get; set; }   // 0~1
        public float SilenceResistChance { get; set; }
        public float RootResistChance { get; set; }
        public float KnockbackResistChance { get; set; }

        public float StunDurationReduce { get; set; }    // 0~1 (예: 0.3f = 30% 감소)
        public float FreezeDurationReduce { get; set; }
        public float SilenceDurationReduce { get; set; }
        public float RootDurationReduce { get; set; }

        public float DefPenFlat { get; set; } = 0f;     // 고정 방관
        public float DefPenPercent { get; set; } = 0f;  // 비율 방관 (0.3f = 30%)
        public float DamageReducePercent => BuffDamageReduce;
        public float FinalDamageMultiplier => 1f - BuffFinalDamageReduce;
        public int DefPenFlatEff => BuffDefPenFlat;
        public float DefPenPercentEff => BuffDefPenPercent;
        public int BuffAtk { get; set; } = 0;
        public int BuffDef { get; set; } = 0;
        public float BuffCritRate { get; set; } = 0f;
        public float BuffCritDamage { get; set; } = 0f;

        public float BuffDamageReduce { get; set; } = 0f;
        public float BuffFinalDamageReduce { get; set; } = 0f;

        public int BuffDefPenFlat { get; set; } = 0;
        public float BuffDefPenPercent { get; set; } = 0f;
        public void RecalcStats()
        {
            // 빈 메서드: Eff 값은 프로퍼티 계산 방식이므로 아무것도 할 필요 없음
        }
    }
}
