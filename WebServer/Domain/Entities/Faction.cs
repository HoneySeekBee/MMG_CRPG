using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Faction
    {
        public int FactionId { get; set; }          // PK
        public string Key { get; set; } = "";       // UNIQUE 권장
        public string Label { get; set; } = "";     // 표시용 이름

        public int? IconId { get; set; }            // FK(옵션)
        public string? ColorHex { get; set; }       // "#RRGGBB" 권장

        public short SortOrder { get; set; }        // 정렬
        public bool IsActive { get; set; } = true;  // 활성/비활성

        /// <summary>임의 메타데이터(JSON 직렬화 문자열)</summary>
        public string? Meta { get; set; }           // DB는 jsonb

        public override string ToString() => string.IsNullOrWhiteSpace(Label) ? Key : Label;
    }
}
