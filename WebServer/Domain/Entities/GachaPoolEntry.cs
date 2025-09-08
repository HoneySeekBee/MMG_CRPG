using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    /// <summary>
    /// 가차풀에서 캐릭터 당 가중치
    /// </summary>
    public sealed class GachaPoolEntry
    {
        private GachaPoolEntry() { }

        public int PoolId { get; private set; }     // FK (EF가 설정)
        public int CharacterId { get; private set; }
        public short Grade { get; private set; }
        public bool RateUp { get; private set; }
        public int Weight { get; private set; }

        public static GachaPoolEntry Create(int characterId, short grade, bool rateUp, int weight)
        {
            if (characterId <= 0) throw new ArgumentOutOfRangeException(nameof(characterId));
            if (weight <= 0) throw new ArgumentOutOfRangeException(nameof(weight), "weight must be positive.");

            return new GachaPoolEntry
            {
                CharacterId = characterId,
                Grade = grade,
                RateUp = rateUp,
                Weight = weight
            };
        }

        public void Update(short grade, bool rateUp, int weight)
        {
            if (weight <= 0) throw new ArgumentOutOfRangeException(nameof(weight), "weight must be positive.");
            Grade = grade;
            RateUp = rateUp;
            Weight = weight;
        }
    }
}
