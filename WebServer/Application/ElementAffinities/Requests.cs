using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ElementAffinities
{
    public record CreateElementAffinityRequest(
        int AttackerElementId,
        int DefenderElementId,
        decimal Multiplier
    );

    public record UpdateElementAffinityRequest(
        decimal Multiplier
    );
}
