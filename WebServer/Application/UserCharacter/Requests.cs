using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UserCharacter
{
    public sealed record CreateUserCharacterRequest(int UserId, int CharacterId);
    public sealed record GainExpRequest(int UserId, int CharacterId, int Amount);
    public sealed record LearnSkillRequest(int UserId, int CharacterId, int SkillId);
    public sealed record LevelUpSkillRequest(int UserId, int CharacterId, int SkillId, int Amount);
    public sealed record GetUserCharacterRequest(int UserId, int CharacterId);
}
