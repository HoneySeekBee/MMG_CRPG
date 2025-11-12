using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Faction
    {
        public int FactionId { get; set; }          
        public string Key { get; set; } = "";       
        public string Label { get; set; } = "";    

        public int? IconId { get; set; }          
        public string? ColorHex { get; set; }   

        public short SortOrder { get; set; }      
        public bool IsActive { get; set; } = true;

        public string? Meta { get; set; }      

        public override string ToString() => string.IsNullOrWhiteSpace(Label) ? Key : Label;
    }
}
