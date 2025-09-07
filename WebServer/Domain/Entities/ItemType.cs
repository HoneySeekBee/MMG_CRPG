using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public sealed class ItemType
    {
        public short Id { get; private set; }
        public string Code { get; private set; } = "";
        public string Name { get; private set; } = "";
        public short? SlotId { get; private set; }  // FK -> EquipSlots.Id (nullable)
        public EquipSlot? Slot { get; private set; } // Nav

        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset UpdatedAt { get; private set; }

        private ItemType() { }

        public ItemType(string code, string name, short? slotId = null)
        {
            if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Code is required.", nameof(code));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));
            Code = code.Trim();
            Name = name.Trim();
            SlotId = slotId;
            CreatedAt = UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void Rename(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(nameof(name));
            Name = name.Trim();
            Touch();
        }
        public void ChangeCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException(nameof(code));
            Code = code.Trim();
            Touch();
        }
        public void SetSlot(short? slotId) { SlotId = slotId; Touch(); }
        private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
    }
}
