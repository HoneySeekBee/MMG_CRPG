using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Skill
{
    public sealed class TargetingEffect
    {
        public int? AoeRange { get; init; }
        public bool IsAoe { get; init; }
        public int? TargetLimit { get; init; }
        public string? TargetGroup { get; init; }  
    }
}
