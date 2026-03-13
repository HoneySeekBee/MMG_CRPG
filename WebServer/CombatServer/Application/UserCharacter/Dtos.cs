using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UserCharacter
{
    public sealed record UserCharacterStatsDto(
       int UserCharacterId,
       int UserId,
       int CharacterId,
       short Level,
       int Hp,
       int Atk,
       int Def,
       int Spd,
       double CritRate,
       double CritDamage,
       float Range
   );
}
