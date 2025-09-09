using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class SynergyRule
    {
        public int SynergyId { get; private set; }
        public Scope Scope { get; private set; }
        public Metric Metric { get; private set; }
        public int RefId { get; private set; }

        public int RequiredCnt { get; private set; } // ← 속성이 int면
        public JsonDocument? Extra { get; private set; }

        public Synergy? Synergy { get; private set; }

        private SynergyRule() { } // EF
                                  // DTO 타입과 맞추기: RequiredCnt를 int로 받도록 수정
        public SynergyRule(Scope scope, Metric metric, int refId, int requiredCnt, JsonDocument? extra)
        {
            Scope = scope;
            Metric = metric;
            RefId = refId;
            RequiredCnt = requiredCnt;
            Extra = extra;
        }
    }

}
