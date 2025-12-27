using Domain.Entities.Skill;
using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Combats
{
    public sealed class CombatSkill
    {
        public int SkillId { get; init; }

        // SkillSystem이 필요로 하는 속성
        public SkillType Type { get; init; }
        public SkillEffect Effect { get; init; } = new();   // 이미 Domain에 SkillEffect가 있다면 OK

        public SkillTargetingType TargetingType { get; init; }
        public TargetSideType TargetSide { get; init; }
        public AoeShapeType AoeShape { get; init; }

        // Targeting 파라미터
        public int TargetLimit { get; init; } = 1;
        public float AoeRange { get; init; } = 0f;
        public float Angle { get; init; } = 0f;
        public float Length { get; init; } = 0f;
        public float Width { get; init; } = 0f;

        // 스킬 실행 파라미터(= BaseInfo 대체)
        public int Hits { get; init; } = 1;

        // AoE 옵션(즉발 AOE 쓰는 경우)
        public bool IsAoe { get; init; } = false;
        public float AoeRadius { get; init; } = 0f;

        // Projectile 옵션
        public bool HasProjectile { get; init; } = false;
        public ProjectileSpec Projectile { get; init; } = ProjectileSpec.Empty;

        // Delayed hit 옵션
        public bool HasDelayedHit { get; init; } = false;
        public DelayedHitSpec DelayedHit { get; init; } = DelayedHitSpec.Empty;
    }

    public readonly record struct ProjectileSpec(
        float Speed,
        int LifetimeMs,
        bool Tracking,
        bool Piercing,
        float AoeRadius,
        int MaxHitCount
    )
    {
        public static readonly ProjectileSpec Empty = new(0, 0, false, false, 0, 0);
    }

    public readonly record struct DelayedHitSpec(float DelaySec, float Multiplier)
    {
        public static readonly DelayedHitSpec Empty = new(0f, 1f);
    }
}
