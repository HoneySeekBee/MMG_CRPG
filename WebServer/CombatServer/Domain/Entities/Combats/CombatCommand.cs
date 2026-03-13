using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Combats
{
    public sealed record CombatCommand(long ActorId, long? TargetActorId, int SkillId, int SkillLevel);
}
