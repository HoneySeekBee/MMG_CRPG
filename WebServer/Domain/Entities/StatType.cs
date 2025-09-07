using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public sealed class StatType
    {
        public short Id { get; private set; }          // PostgreSQL smallint ↔ C# short
        public string Code { get; private set; } = ""; // UNIQUE
        public string Name { get; private set; } = "";
        public bool IsPercent { get; private set; }

        private StatType() { } // EF용

        public StatType(string code, string name, bool isPercent)
        {
            if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException(nameof(code));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(nameof(name));
            Code = code.Trim();
            Name = name.Trim();
            IsPercent = isPercent;
        }

        public void Rename(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(nameof(name));
            Name = name.Trim();
        }

        public void ChangeCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException(nameof(code));
            Code = code.Trim();
        }

        public void SetPercent(bool isPercent) => IsPercent = isPercent;
    }
}
