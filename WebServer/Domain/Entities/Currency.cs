using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public sealed class Currency
    {
        public int Id { get; private set; }          // smallint
        public string Code { get; private set; } = ""; // UNIQUE
        public string Name { get; private set; } = "";

        private Currency() { }
        public Currency(string code, string name)
        {
            if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException(nameof(code));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(nameof(name));
            Code = code.Trim();
            Name = name.Trim();
        }

        public void Rename(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(nameof(name));
            Name = name.Trim();
        }
    }
}
