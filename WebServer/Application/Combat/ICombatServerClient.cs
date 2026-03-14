using Application.Combat.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Combat;

public interface ICombatServerClient
{
    Task<CombatInitialSnapshotPayload> InitCombatAsync(InitCombatPayload payload, CancellationToken ct);
    Task<CombatResultPayload> GetResultAsync(long combatId, CancellationToken ct);
}
