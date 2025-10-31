using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Characters
{
    public sealed class CharacterExp
    {
        public short RarityId { get; }
        public short Level { get; }
        public int RequiredExp { get; }
        private CharacterExp() { }
    }
    public interface ICharacterExpProvider
    {
        int GetRequiredExp(short rarityId, int level);
    }
}
