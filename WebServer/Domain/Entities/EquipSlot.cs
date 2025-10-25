using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public sealed class EquipSlot
    {
        public short Id { get; private set; }
        public string Code { get; private set; } = "";
        public string Name { get; private set; } = "";
        public short SortOrder { get; private set; }
        public int IconId { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset UpdatedAt { get; private set; }
        private EquipSlot() { }
    }
}
