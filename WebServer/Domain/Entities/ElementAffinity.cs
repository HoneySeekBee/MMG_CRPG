using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ElementAffinity
    {
        public int AttackerElementId{ get; set; }
        public int DefenderElementId{ get; set; }

        public decimal Multiplier { get; set; } = 1.00m;
    }
}
