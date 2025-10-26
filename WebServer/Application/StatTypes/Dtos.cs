using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.StatTypes
{
    public sealed record StatTypeDto(int Id, string Code, string Name, bool IsPercent);

    public sealed record CreateStatTypeRequest(string Code, string Name, bool IsPercent);
    public sealed record UpdateStatTypeRequest(string? Code, string? Name, bool? IsPercent);
}
