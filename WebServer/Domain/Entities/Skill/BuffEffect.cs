using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Skill
{
    public sealed record BuffEffect
    {
        public int SkillId { get; init; }
         
        public BuffKind Kind { get; init; }

        public float Value { get; init; }  
        public int DurationMs { get; init; }  
        public int MaxStacks { get; init; } = 1;

        public int? ShieldValue { get; init; } 

    }
}
