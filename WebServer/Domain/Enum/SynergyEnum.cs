using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enum
{
    public enum Stacking : short   // Synergy.Stacking
    {
        None = 0,          // 중복 불가
        Additive = 1,      // 합연산
        Multiplicative = 2,// 곱연산
        HighestOnly = 3    // 최대치 1개만 적용
    }

    public enum Scope : short      // SynergyRule.Scope
    {
        Party = 0,     // 파티 전체
        Character = 1  // 개별 캐릭터(장착 장비 기준)
    }

    public enum Metric : short     // SynergyRule.Metric
    {
        PartyElement = 0,   // RefId = ElementId
        PartyFaction = 1,   // RefId = FactionId
        CharacterItemTag = 2// RefId = ItemSetId (착용 세트)
    }

    public enum TargetType : short // SynergyTarget.TargetType
    {
        Party = 0, Ally = 1, Self = 2, Position = 3, Faction = 4, Element = 5, Role = 6, Tag = 7
    }
}
