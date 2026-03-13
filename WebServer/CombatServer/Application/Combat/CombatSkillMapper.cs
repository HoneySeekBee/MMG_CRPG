using Application.Skills;
using Domain.Entities.Combats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Combat
{
    public static class CombatSkillMapper
    {
        public static CombatSkill ToCombatSkill(SkillWithLevelsDto dto)
        {
            var baseInfo = dto.BaseInfo;

            // 1) AoE 미리 계산
            var aoe = baseInfo?["aoe"];
            bool isAoe = aoe is not null;
            float aoeRadius = aoe?["radius"]?.GetValue<float>() ?? 0f;

            // 2) Projectile 미리 계산
            var proj = baseInfo?["projectile"];
            bool hasProjectile = proj is not null;

            float speed = proj?["speed"]?.GetValue<float>() ?? 0f;
            int lifetime = proj?["lifetime"]?.GetValue<int>() ?? 0;
            bool tracking = proj?["tracking"]?.GetValue<bool>() ?? false;

            bool piercing = proj?["pierce"]?.GetValue<bool>() ?? false;
            float projAoeRadius = proj?["aoeRadius"]?.GetValue<float>() ?? 0f;
            int maxHit = proj?["maxHit"]?.GetValue<int>() ?? 1;

            var projectileSpec = hasProjectile
                ? new ProjectileSpec(
                    Speed: speed,
                    LifetimeMs: lifetime,
                    Tracking: tracking,
                    Piercing: piercing,
                    AoeRadius: projAoeRadius,
                    MaxHitCount: maxHit
                )
                : ProjectileSpec.Empty;

            // 3) DelayedHit 미리 계산
            var delayed = baseInfo?["extra"]?["delayedHit"];
            bool hasDelayedHit = delayed is not null;

            float delaySec = delayed?["delay"]?.GetValue<float>() ?? 0f;
            float multiplier = delayed?["multiplier"]?.GetValue<float>() ?? 1f;

            var delayedSpec = hasDelayedHit
                ? new DelayedHitSpec(delaySec, multiplier)
                : DelayedHitSpec.Empty;

            // 4) 최종 CombatSkill 생성
            return new CombatSkill
            {
                SkillId = dto.SkillId,
                Type = dto.Type,
                TargetingType = dto.TargetingType,
                TargetSide = dto.TargetSide,
                AoeShape = dto.AoeShape,
                Effect = dto.Effect,

                TargetLimit = baseInfo?["targetLimit"]?.GetValue<int>() ?? 1,
                AoeRange = baseInfo?["aoeRange"]?.GetValue<float>() ?? 0f,
                Angle = baseInfo?["angle"]?.GetValue<float>() ?? 0f,
                Length = baseInfo?["length"]?.GetValue<float>() ?? 0f,
                Width = baseInfo?["width"]?.GetValue<float>() ?? 0f,

                Hits = baseInfo?["hits"]?.GetValue<int>() ?? 1,

                IsAoe = isAoe,
                AoeRadius = aoeRadius,

                HasProjectile = hasProjectile,
                Projectile = projectileSpec,

                HasDelayedHit = hasDelayedHit,
                DelayedHit = delayedSpec,
            };
        }
    }
}
