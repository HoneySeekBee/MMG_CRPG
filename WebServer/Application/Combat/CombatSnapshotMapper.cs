using Domain.Entities.Combats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Combat
{
    public static class CombatSnapshotMapper
    {
        public static CombatSnapshotDto ToDto(CombatSnapshot snap)
        {
            var dto = new CombatSnapshotDto();

            foreach (var a in snap.Actors)
            {
                dto.Actors.Add(new ActorSnapshotDto
                {
                    ActorId = a.ActorId,
                    X = a.X,
                    Z = a.Z,
                    Hp = a.Hp,
                    Dead = a.Dead,
                });
            }

            return dto;
        }
    }
}