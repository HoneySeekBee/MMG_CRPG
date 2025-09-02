using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Rarities
{
    public sealed class CreateRarityRequest
    {
        public short Stars { get; set; }          // DB 컬럼명과 동일하게
        public string Key { get; set; } = "";
        public string Label { get; set; } = "";
        public string? ColorHex { get; set; }
        public short SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Meta { get; set; }
    }
    public sealed class UpdateRarityRequest
    {
        public short Stars { get; set; }          // 수정 가능하도록 둠(필요 없으면 제거)
        public string Label { get; set; } = "";
        public string? ColorHex { get; set; }
        public short SortOrder { get; set; }
        public bool IsActive { get; set; }
        public string? Meta { get; set; }
    }
}
