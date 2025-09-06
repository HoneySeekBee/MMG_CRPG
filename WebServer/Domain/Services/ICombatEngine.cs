using Domain.Entities;
using Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services
{
    public interface ICombatEngine
    {
        CombatEngineResult Simulate(
            CombatInputSnapshot input,
            long seed,
            MasterDataPack master // 마스터 데이터(읽기 전용) 패키지
        );
    }
    public sealed record CombatEngineResult(
        Enum.CombatResult Result,
        int ClearMs,
        IReadOnlyList<CombatLogEvent> Events
    );

    public sealed record MasterDataPack(
        StageDef Stage,
        IReadOnlyDictionary<long, CharacterDef> Characters,
        IReadOnlyDictionary<long, SkillDef> Skills
    );

    public sealed record StageDef(
    long StageId,
        IReadOnlyList<EnemySpawn> Enemies
    );

    public sealed record EnemySpawn(long CharacterId, int Level);

    public sealed record CharacterDef(
        long CharacterId,
        int BaseHp,
        int BaseAtk,
        int BaseDef,
        int BaseAspd,     // 공격 주기(ms)
        float CritRate,   // 0~1
        float CritDmg     // 0~(ex: 1.5 = +150%)
    );
    public sealed record SkillDef(
        long SkillId,
        int CooldownMs,
        float Coeff // 피해 계수 등 (샘플 단순화)
    );
}
