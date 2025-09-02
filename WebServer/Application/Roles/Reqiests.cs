using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Roles
{
    public sealed class CreateRoleRequest
    {
        public string Key { get; set; } = "";
        public string Label { get; set; } = "";
        public int? IconId { get; set; }
        public string? ColorHex { get; set; }
        public short SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Meta { get; set; }
    }
    public sealed class UpdateRoleRequest
    {
        public string Label { get; set; } = "";
        public int? IconId { get; set; }
        public string? ColorHex { get; set; }
        public short SortOrder { get; set; }
        public bool IsActive { get; set; }
        public string? Meta { get; set; }
    }
}
