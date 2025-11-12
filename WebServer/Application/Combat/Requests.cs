using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Combat
{
    public sealed record SimulateCombatRequest(
int StageId,
long? Seed,
IReadOnlyList<PartyMemberDto> Party,
IReadOnlyList<SkillInputDto> SkillInputs,
string? ClientVersion
);


    public sealed record PartyMemberDto(long CharacterId, int Level);
    public sealed record SkillInputDto(int TMs, string CasterRef, long SkillId, IReadOnlyList<string> Targets);
    public sealed record SimulateCombatResponse(
    long CombatId,
    string Result,     // "win" | "lose" | "error"
    int? ClearMs,
    string? BalanceVersion,
    string? ClientVersion,
    IReadOnlyList<CombatLogEventDto> Events // 짧게만 보내고 나머진 GetLog
);

}
