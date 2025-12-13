using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Application.SkillLevels
{
    public sealed class CreateSkillLevelRequest
    {
        public int SkillId { get; set; }
        public int Level { get; set; }

        // 자유 확장 값들 (계수/지속/추가 이펙트 등)
        public JsonNode? Values { get; set; }

        // 설명(툴/클라이언트 표시용) 
        public string? Description { get; set; }

        // 재료 (itemId -> count) 
        public IDictionary<string, int>? Materials { get; set; }

        // 강화/해금 비용 
        public int CostGold { get; set; }
    }
    public sealed class UpdateSkillLevelRequest
    {
        public JsonNode? Values { get; set; }
        public string? Description { get; set; }
        public IDictionary<string, int>? Materials { get; set; }
        public int CostGold { get; set; }
    }
}
