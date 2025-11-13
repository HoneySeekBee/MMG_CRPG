using Application.Combat;
using Combat;
using Google.Protobuf.WellKnownTypes;

namespace WebServer.Mappers
{
    public static class CombatProtoMapper
    {
        public static StartCombatResponsePb ToPb(Application.Combat.StartCombatResponse src)
        {
            var snapshotPb = new CombatInitialSnapshotPb();

            foreach (var a in src.Snapshot.Actors)
            {
                snapshotPb.Actors.Add(new ActorInitPb
                {
                    ActorId = a.ActorId,
                    Team = a.Team,
                    X = a.X,
                    Z = a.Z,
                    Hp = a.Hp,
                    MaxHp = a.MaxHp,
                    ModelCode = a.ModelCode
                });
            }

            return new StartCombatResponsePb
            {
                CombatId = src.CombatId,
                Snapshot = snapshotPb
            };
        }

        public static Application.Combat.CombatCommandDto ToDomain(CombatCommandPb pb)
        {
            return new Application.Combat.CombatCommandDto(
                pb.ActorId,
                pb.SkillId,
                pb.TargetActorId.Value
            );
        }

        public static CombatLogPagePb ToPb(Application.Combat.CombatLogPageDto src)
        {
            var pb = new CombatLogPagePb
            {
                CombatId = src.CombatId
            };

            foreach (var e in src.Items)
            {
                var evPb = new CombatLogEventPb
                {
                    TMs = e.TMs,
                    Type = e.Type,
                    Actor = e.Actor
                };

                if (e.Target != null)
                    evPb.Target = e.Target;

                if (e.Damage.HasValue)
                    evPb.Damage = e.Damage.Value;

                if (e.Crit.HasValue)
                    evPb.Crit = e.Crit.Value;

                if (e.Extra != null)
                {
                    var structVal = new Struct();
                    foreach (var kv in e.Extra)
                        structVal.Fields[kv.Key] = Value.ForString(kv.Value?.ToString() ?? "");

                    evPb.Extra = structVal;
                }

                pb.Items.Add(evPb);
            }

            if (src.NextCursor != null)
                pb.NextCursor = src.NextCursor;

            return pb;
        }

        public static CombatLogSummaryPb ToPb(Application.Combat.CombatLogSummaryDto src)
        {
            return new CombatLogSummaryPb
            {
                CombatId = src.CombatId,
                TotalEvents = src.TotalEvents,
                DurationMs = src.DurationMs,
                DamageDone = src.DamageDone,
                DamageTaken = src.DamageTaken
            };
        }
    }
}
