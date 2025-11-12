using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ElementAffinities
{
    public record ElementAffinityDto
    {

        public int AttackerElementId { get; set; }
        public string AttackerElementLabel { get; set; } = "";
        public int DefenderElementId { get; set; }
        public string DefenderElementLabel { get; set; } = "";
        public decimal Multiplier { get; set; }
        public static ElementAffinityDto From(ElementAffinity e)
        {
            return new ElementAffinityDto
            {
                AttackerElementId = e.AttackerElementId,
                DefenderElementId = e.DefenderElementId,
                Multiplier = e.Multiplier
            };
        }

    }
}
