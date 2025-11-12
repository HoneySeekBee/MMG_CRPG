using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Portrait
    {
        public int PortraitId { get; set; }
        public string Key { get; set; } = string.Empty;

        public string? Atlas { get; set; }
        public int? X { get; set; }
        public int? Y { get; set; }
        public int? W { get; set; }
        public int? H { get; set; }

        public int Version { get; set; } = 1;
    }
}
