using Application.Combat.Runtime;
using Application.Skills;
using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Combat.Engine.TickSystems.Skill
{
    public static class TargetSelector
    {
        public static List<ActorState> SelectTargets(
            CombatRuntimeState state,
            ActorState caster,
            SkillWithLevelsDto skill)
        {
            var actors = state.ActiveActors.Values
                .Where(a => !a.Dead && a.Hp > 0)
                .ToList();

            actors = skill.TargetSide switch
            {
                TargetSideType.Team => actors.Where(a => a.Team == caster.Team).ToList(),
                TargetSideType.Enemy => actors.Where(a => a.Team != caster.Team).ToList(),
                _ => new List<ActorState>(),
            };

            if (actors.Count == 0)
                return new List<ActorState>();
            return skill.TargetingType switch
            {
                SkillTargetingType.Single =>
                    SelectNearest(caster, actors, 1),

                SkillTargetingType.SingleNearest =>
                    SelectNearest(caster, actors, 1),

                SkillTargetingType.SingleFarthest =>
                    SelectFarthest(caster, actors, 1),

                SkillTargetingType.LowestHp =>
                    actors.OrderBy(a => a.Hp).Take(1).ToList(),

                SkillTargetingType.HighestAtk =>
                    actors.OrderByDescending(a => a.AtkEff).Take(1).ToList(),

                SkillTargetingType.RandomOne =>
                    actors.OrderBy(_ => Guid.NewGuid()).Take(1).ToList(),

                // N-targets
                SkillTargetingType.NearestN =>
                    SelectNearest(caster, actors, GetTargetLimit(skill)),

                SkillTargetingType.FarthestN =>
                    SelectFarthest(caster, actors, GetTargetLimit(skill)),

                SkillTargetingType.FrontN =>
                    SortFrontToBack(actors)
                        .Take(GetTargetLimit(skill)).ToList(),

                SkillTargetingType.BackN =>
                    SortBackToFront(actors)
                        .Take(GetTargetLimit(skill)).ToList(),

                // Entire group
                SkillTargetingType.AllEnemies =>
                    actors,

                SkillTargetingType.AllAllies =>
                    actors,

                // AOE
                SkillTargetingType.AreaCircle =>
                    ApplyCircleAoe(caster, actors, skill),

                SkillTargetingType.AreaSector =>
                    ApplySectorAoe(caster, actors, skill),

                SkillTargetingType.AreaRectangle =>
                    ApplyRectangleAoe(caster, actors, skill),

                _ => SelectNearest(caster, actors, 1),
            };
        }
        private static List<ActorState> SelectNearest(
            ActorState caster, List<ActorState> actors, int count)
        {
            return actors.OrderBy(a => Dist(caster, a))
                         .Take(count)
                         .ToList();
        }
        private static List<ActorState> SelectFarthest(
            ActorState caster, List<ActorState> actors, int count)
        {
            return actors.OrderByDescending(a => Dist(caster, a))
                         .Take(count)
                         .ToList();
        }
        private static float Dist(ActorState a, ActorState b)
        {
            float dx = a.X - b.X;
            float dz = a.Z - b.Z;
            return MathF.Sqrt(dx * dx + dz * dz);
        }
        private static int GetTargetLimit(SkillWithLevelsDto skill)
        {
            var baseInfo = skill.BaseInfo;
            if (baseInfo?["targetLimit"] != null)
                return (int)baseInfo["targetLimit"]!;
            return 1;
        }
        private static IEnumerable<ActorState> SortBackToFront(List<ActorState> actors)
        {
            return actors.OrderByDescending(a => a.Z);
        }


        private static IEnumerable<ActorState> SortFrontToBack(List<ActorState> actors)
        {
            // Z 축 기준 오름차순 → 앞(작은 z) → 뒤(큰 z)
            return actors.OrderBy(a => a.Z);
        }
        private static List<ActorState> ApplyCircleAoe(
          ActorState caster,
          List<ActorState> actors,
          SkillWithLevelsDto skill)
        {
            float radius = skill.BaseInfo?["aoeRange"]?.GetValue<float>() ?? 1.5f;

            return actors
                .Where(t => Dist(caster, t) <= radius)
                .ToList();
        }
        private static List<ActorState> ApplySectorAoe(
            ActorState caster,
            List<ActorState> actors,
            SkillWithLevelsDto skill)
        {
            float radius = skill.BaseInfo?["aoeRange"]?.GetValue<float>() ?? 3f;
            float angle = skill.BaseInfo?["angle"]?.GetValue<float>() ?? 60f;

            float forwardX = caster.FacingX;
            float forwardZ = caster.FacingZ;

            // normalize
            float fl = MathF.Sqrt(forwardX * forwardX + forwardZ * forwardZ);
            if (fl > 0.0001f)
            {
                forwardX /= fl;
                forwardZ /= fl;
            }

            List<ActorState> result = new();

            foreach (var t in actors)
            {
                float dist = Dist(caster, t);
                if (dist > radius) continue;

                float dx = t.X - caster.X;
                float dz = t.Z - caster.Z;

                float dot = dx * forwardX + dz * forwardZ;
                float len = MathF.Sqrt(dx * dx + dz * dz);

                float cos = dot / (len + 0.0001f);
                float deg = MathF.Acos(cos) * (180f / MathF.PI);

                if (deg <= angle / 2f)
                    result.Add(t);
            }

            return result;
        }
        private static List<ActorState> ApplyRectangleAoe(
            ActorState caster,
            List<ActorState> actors,
            SkillWithLevelsDto skill)
        {
            float length = skill.BaseInfo?["length"]?.GetValue<float>() ?? 4f;
            float width = skill.BaseInfo?["width"]?.GetValue<float>() ?? 1.5f;

            float forwardX = caster.FacingX;
            float forwardZ = caster.FacingZ;

            float fl = MathF.Sqrt(forwardX * forwardX + forwardZ * forwardZ);
            if (fl > 0.0001f)
            {
                forwardX /= fl;
                forwardZ /= fl;
            }
            List<ActorState> result = new();

            foreach (var t in actors)
            {
                float dx = t.X - caster.X;
                float dz = t.Z - caster.Z;

                float dot = dx * forwardX + dz * forwardZ;    // 전방 거리
                float cross = dx * forwardZ - dz * forwardX;  // 좌우 오프셋

                if (dot < 0) continue;
                if (dot > length) continue;
                if (MathF.Abs(cross) > width * 0.5f) continue;

                result.Add(t);
            }

            return result;
        }
    }
}
